using System.Text.Json;
using System.Text.Json.Serialization;
using CodeBaseContextGenerator.JavaAntlr4.Visitors;

namespace CodeBaseContextGenerator;

class Program
{
    // 📄 Output lives one level above solution root so it’s easy to inspect
    private static readonly string JsonPath = Path.Combine(
        Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!.Parent!.FullName,
        "project_context.json");

    private static OllamaClient ollama = new();

    private static string _chosenPath = string.Empty;

    static async Task Main(string[] args)
    {
        // 1️⃣ Let the user pick a .java file or a folder (WASD file‑explorer)
        _chosenPath = FileExplorer.Browse();

        // 2️⃣ Parse + emit JSON (blocking). Hit any key to re‑run.
        await InspectJavaPath(_chosenPath);
        
        ProjectExplorer.BrowseStructure("../../../project_context.json");
        /*while (true)
        {
            
            Console.WriteLine("Press any key to re‑load & re‑analyze …");
            Console.ReadKey(true);
            await InspectJavaPath(_chosenPath);
        }*/
    }

    /*─────────────────────────  Core  ─────────────────────────*/

    private static async Task InspectJavaPath(string path)
    {
        var allItems = new List<TypeRepresentation>();
        var rootPath = File.Exists(path) ? Path.GetDirectoryName(path)! : path;

        var inspector = new JavaAstInspector(rootPath);

        if (File.Exists(path) && Path.GetExtension(path) == ".java")
        {
            allItems.AddRange(inspector.Inspect(path));
        }
        else if (Directory.Exists(path))
        {
            foreach (var file in Directory.GetFiles(path, "*.java", SearchOption.AllDirectories))
                allItems.AddRange(inspector.Inspect(file));
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("✖  Invalid path. Please choose a .java file or folder.");
            Console.ResetColor();
            return;
        }

        ResolveTypeReferences(allItems);
        var grouped = GroupByFile(allItems, rootPath);

        int changeCount;
        if ((changeCount = await UpdateJsonIfChanged(JsonPath, grouped)) == 0)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("✖  No changes detected. Nothing to update.");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓  JSON file updated {changeCount} out of {allItems.Count} modified elements.");
        }

        Console.ResetColor();
    }

    /*─────────────────────────  Helper Utils  ─────────────────────────*/

    private static void ResolveTypeReferences(List<TypeRepresentation> allItems)
    {
        var allTypesByName = allItems
            .Where(t => t.Type != "method")
            .GroupBy(t => t.Name)
            .ToDictionary(g => g.Key, g =>
            {
                if (g.Count() > 1)
                {
                    Console.WriteLine($"[Warning] Multiple definitions for '{g.Key}':");
                    foreach (var entry in g)
                        Console.WriteLine($"    ↳ {entry.SourcePath}");
                }

                return g.First();
            });

        foreach (var type in allItems)
        {
            ResolveRefs(type.ReferencedTypes, type.Name);
            if (type.Methods == null) continue;
            foreach (var method in type.Methods)
                ResolveRefs(method.ReferencedTypes, method.Name);
        }

        void ResolveRefs(IEnumerable<TypeReference>? refs, string owner)
        {
            if (refs == null) return;
            foreach (var r in refs)
            {
                if (allTypesByName.TryGetValue(r.Name, out var target))
                    r.Source = $"{r.Name}@{target.SourcePath}";
                else
                    Console.WriteLine($"[Warning] Unresolved reference {r.Name} in {owner}");
            }
        }
    }

    private static Dictionary<string, List<TypeRepresentation>> GroupByFile(
        IEnumerable<TypeRepresentation> items, string rootPath)
    {
        // Use the existing relative SourcePath on each item (already computed by the builder)
        var groups = new Dictionary<string, List<TypeRepresentation>>();

        foreach (var item in items)
        {
            // SourcePath is relative to the selected root, with forward slashes
            var relPath = item.SourcePath.Replace('\'', '/');
            var fileName = Path.GetFileName(relPath);
            var dir = Path.GetDirectoryName(relPath)?.Replace('\'', '/') ?? string.Empty;
            if (string.IsNullOrEmpty(dir)) dir = ".";

            var key = $"{fileName}@{dir}";
            if (!groups.TryGetValue(key, out var list))
            {
                list = new List<TypeRepresentation>();
                groups[key] = list;
            }

            list.Add(item);
        }

        return groups;
    }

    private static async Task<int> UpdateJsonIfChanged(
        string outputPath,
        Dictionary<string, List<TypeRepresentation>> grouped)
    {
        int changeCount = 0;
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        // 1) Load existing JSON into a map: fileKey -> list of TypeRepresentation
        Dictionary<string, List<TypeRepresentation>>? existing = null;
        if (File.Exists(outputPath))
        {
            try
            {
                var raw = File.ReadAllText(outputPath);
                existing = JsonSerializer.Deserialize<
                        List<Dictionary<string, List<TypeRepresentation>>>>(raw)
                    ?.SelectMany(d => d)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }
            catch
            {
                Console.WriteLine("⚠ Could not parse existing JSON; treating as empty.");
            }
        }

        // 2) Detect removed file‑groups
        if (existing != null)
        {
            foreach (var oldKey in existing.Keys.Except(grouped.Keys))
            {
                Console.WriteLine($"✖ File removed: {oldKey}");
                changeCount++;
            }
        }

        var finalOutput = new List<Dictionary<string, List<TypeRepresentation>>>();
        bool anyChanges = changeCount > 0;

        // 3) Walk each current file‑group
        foreach (var kvp in grouped)
        {
            string fileKey = kvp.Key;
            var newTypes = kvp.Value;
            var oldTypes = existing != null && existing.TryGetValue(fileKey, out var tmp)
                ? tmp
                : new List<TypeRepresentation>();

            // 3a) Detect removed top‑level types (classes/interfaces)
            foreach (var removedType in oldTypes
                         .Where(o => !newTypes.Any(n => n.Name == o.Name && n.Type == o.Type)))
            {
                Console.WriteLine($"✖ Removed type: {removedType.Name} ({removedType.Type}) in {fileKey}");
                changeCount++;
                anyChanges = true;
            }

            var updatedTypes = new List<TypeRepresentation>();

            // 3b) Compare & merge each new type
            foreach (var newType in newTypes)
            {
                var oldType = oldTypes
                    .FirstOrDefault(o => o.Name == newType.Name && o.Type == newType.Type);

                bool typeChanged = oldType == null || oldType.Hash != newType.Hash;
                TypeRepresentation chosen;

                // 3b‑i) If type signature unchanged, dig into methods
                if (!typeChanged
                    && oldType.Methods != null
                    && newType.Methods != null)
                {
                    // Detect removed methods
                    foreach (var removedMethod in oldType.Methods
                                 .Where(m => !newType.Methods.Any(nm => nm.Name == m.Name && nm.Type == "method")))
                    {
                        Console.WriteLine($"  ✖ Removed method: {removedMethod.Name} in {newType.Name}");
                        changeCount++;
                        anyChanges = true;
                    }

                    var mergedMethods = new List<TypeRepresentation>();
                    bool methodChangedAny = false;

                    // Compare each new method
                    foreach (var newMethod in newType.Methods)
                    {
                        var oldMethod = oldType.Methods
                            .FirstOrDefault(m => m.Name == newMethod.Name && m.Type == "method");

                        if (oldMethod != null && oldMethod.Hash == newMethod.Hash)
                        {
                            mergedMethods.Add(oldMethod);
                            Console.WriteLine($"  ✓ No changes in method {newMethod.Name}");
                        }
                        else
                        {
                            var prompt = $@"
You are generating structured summaries of Java code for automated analysis.

Instructions:
- Use this format (no deviations):

Purpose:[high-level intent], Behavior:[specific operations or responsibilities]

Rules:
- DO NOT include class or method names
- DO NOT include parameter names or types
- DO NOT quote or refer to source code
- Use plain English, lowercase, complete sentences
- Use general terms like “this class handles authentication logic”
- Keep Behavior concise but informative, describing what the code does
- If the code is too simple or unclear, still return a complete sentence for Behavior

Java code:
{newType.Code}
";

                            var rawSummary = await ollama.SendPromptAsync("llama3", prompt);
                            rawSummary = rawSummary.Replace("\"", "") // remove escaped double-quotes
                                .Replace("'", "") // remove single quotes if needed
                                .Replace("\n", " ") // replace newlines with space
                                .Replace("\r", " ") // replace carriage returns with space
                                .Trim();
                            string cleanedSummary = rawSummary;

                            var purposeIndex = rawSummary.IndexOf("Purpose:", StringComparison.OrdinalIgnoreCase);
                            if (purposeIndex >= 0)
                            {
                                cleanedSummary = rawSummary.Substring(purposeIndex).Trim();
                            }
                            else
                            {
                                Console.WriteLine("⚠️ Warning: 'Purpose=' not found in summary.");
                            }

                            newMethod.Summary = cleanedSummary;
                            mergedMethods.Add(newMethod);
                            Console.WriteLine($"  ✖ Method changed or added: {newMethod.Name}");
                            methodChangedAny = true;
                            changeCount++;
                            anyChanges = true;
                        }
                    }

                    if (!methodChangedAny)
                    {
                        Console.WriteLine($"✓ No changes in {newType.Name} ({newType.Type})");
                        chosen = oldType;
                    }
                    else
                    {
                        Console.WriteLine($"✖ Changes detected in methods of {newType.Name} ({newType.Type})");
                        // produce a copy with updated methods
                        chosen = new TypeRepresentation
                        {
                            Name = newType.Name,
                            Type = newType.Type,
                            Privacy = newType.Privacy,
                            ReturnType = newType.ReturnType,
                            Parameters = newType.Parameters,
                            Code = newType.Code,
                            SourcePath = newType.SourcePath,
                            ReferencedTypes = newType.ReferencedTypes,
                            Methods = mergedMethods
                        };
                    }
                }
                // 3b‑ii) If the type signature changed (or is entirely new)
                else if (typeChanged)
                {
                    Console.WriteLine($"✖ Structural change or new type: {newType.Name} ({newType.Type})");
                    changeCount++;
                    anyChanges = true;

// Summarize the class/interface
                    var typePrompt = $@"
You are generating structured summaries of Java code for automated analysis.

Instructions:
- Use this format (no deviations):

Purpose:[high-level intent], Behavior:[specific operations or responsibilities]

Rules:
- DO NOT include class or method names
- DO NOT include parameter names or types
- DO NOT quote or refer to source code
- Use plain English, lowercase, complete sentences
- Use general terms like “this class handles authentication logic”
- Keep Behavior concise but informative, describing what the code does
- If the code is too simple or unclear, still return a complete sentence for Behavior

Java code:
{newType.Code}
";
                    var rawSummaryType = await ollama.SendPromptAsync("llama3", typePrompt);
                    rawSummaryType = rawSummaryType.Replace("\"", "") // remove escaped double-quotes
                        .Replace("'", "") // remove single quotes if needed
                        .Replace("\n", " ") // replace newlines with space
                        .Replace("\r", " ") // replace carriage returns with space
                        .Trim();
                    string cleanedSummaryType = rawSummaryType;

                    var purposeIndexType = rawSummaryType.IndexOf("Purpose:", StringComparison.OrdinalIgnoreCase);
                    if (purposeIndexType >= 0)
                    {
                        cleanedSummaryType = rawSummaryType.Substring(purposeIndexType).Trim();
                    }
                    else
                    {
                        Console.WriteLine("⚠️ Warning: 'Purpose=' not found in summary.");
                    }


// Diff methods manually, even if the parent structure changed
                    var mergedMethods = new List<TypeRepresentation>();

                    if (newType.Methods != null)
                    {
                        foreach (var method in newType.Methods)
                        {
                            var oldMethod =
                                oldType?.Methods?.FirstOrDefault(m => m.Name == method.Name && m.Type == "method");

                            if (oldMethod != null && oldMethod.Hash == method.Hash)
                            {
                                mergedMethods.Add(oldMethod);
                                Console.WriteLine($"  ✓ No changes in method {method.Name}");
                            }
                            else
                            {
                                var methodPrompt = $@"
You are generating structured summaries of Java code for automated analysis.

Instructions:
- Use this format (no deviations):

Purpose:[high-level intent], Behavior:[specific operations or responsibilities]

Rules:
- DO NOT include class or method names
- DO NOT include parameter names or types
- DO NOT quote or refer to source code
- Use plain English, lowercase, complete sentences
- Use general terms like “this class handles authentication logic”
- Keep Behavior concise but informative, describing what the code does
- If the code is too simple or unclear, still return a complete sentence for Behavior

Java code:
{method.Code}
";
                                var rawSummary = await ollama.SendPromptAsync("llama3", methodPrompt);
                                rawSummary = rawSummary.Replace("\"", "") // remove escaped double-quotes
                                    .Replace("'", "") // remove single quotes if needed
                                    .Replace("\n", " ") // replace newlines with space
                                    .Replace("\r", " ") // replace carriage returns with space
                                    .Trim();
                                string cleanedSummary = rawSummary;

                                var purposeIndex = rawSummary.IndexOf("Purpose:", StringComparison.OrdinalIgnoreCase);
                                if (purposeIndex >= 0)
                                {
                                    cleanedSummary = rawSummary.Substring(purposeIndex).Trim();
                                }
                                else
                                {
                                    Console.WriteLine("⚠️ Warning: 'Purpose=' not found in summary.");
                                }

                                method.Summary = cleanedSummary;
                                mergedMethods.Add(method);
                                Console.WriteLine($"  ✖ Method changed or added: {method.Name}");
                                changeCount++;
                            }
                        }
                    }

// Finalize updated structure
                    chosen = new TypeRepresentation
                    {
                        Name = newType.Name,
                        Type = newType.Type,
                        Privacy = newType.Privacy,
                        ReturnType = newType.ReturnType,
                        Parameters = newType.Parameters,
                        Code = newType.Code,
                        SourcePath = newType.SourcePath,
                        ReferencedTypes = newType.ReferencedTypes,
                        Summary = cleanedSummaryType,
                        Methods = mergedMethods
                    };
                }
                // 3b‑iii) Perfect match: reuse old
                else
                {
                    Console.WriteLine($"✓ No changes in {newType.Name} ({newType.Type})");
                    chosen = oldType!;
                }

                updatedTypes.Add(chosen);
            }

            finalOutput.Add(new Dictionary<string, List<TypeRepresentation>>
            {
                [fileKey] = updatedTypes
            });
        }

        // 4) If nothing changed, skip write
        if (!anyChanges)
        {
            Console.WriteLine("✓ No class/interface/method changes. JSON file unchanged.");
            return changeCount;
        }

        // 5) Write out the merged result
        var json = JsonSerializer.Serialize(finalOutput, options);
        File.WriteAllText(outputPath, json);
        Console.WriteLine("✓ JSON file updated. Total changes: " + changeCount);

        GenerateProgramSummary(Path.Combine(
            Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!.Parent!.FullName,
            "project_context.json"), Path.Combine(
            Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!.Parent!.FullName,
            "program_summary.txt"));
        return changeCount;
    }

    private static void GenerateProgramSummary(string inputPath, string outputPath)
    {
        if (!File.Exists(inputPath))
        {
            Console.WriteLine("❌ JSON file not found.");
            return;
        }

        var raw = File.ReadAllText(inputPath);
        var parsed = JsonSerializer.Deserialize<List<Dictionary<string, List<TypeRepresentation>>>>(raw);
        if (parsed == null)
        {
            Console.WriteLine("❌ Failed to parse JSON.");
            return;
        }

        var lines = new List<string>();

        foreach (var fileGroup in parsed.SelectMany(dict => dict))
        {
            string filePath = fileGroup.Key;

            foreach (var item in fileGroup.Value)
            {
                string prefix = $"[{item.Type}]@{item.SourcePath}";
                string header = $"{item.Privacy} {item.ReturnType ?? ""} {item.Name} {item.Parameters ?? "()"}".Trim();
                string summary = item.Summary?.Trim() ?? "No summary";
                string references = item.ReferencedTypes != null && item.ReferencedTypes.Any()
                    ? string.Join(", ", item.ReferencedTypes.Select(r => r.Name))
                    : "None";

                lines.Add($"{prefix}: {header} {summary} References: {references}");

                // Optionally include methods (if this is a class/interface)
                if (item.Methods != null)
                {
                    foreach (var method in item.Methods)
                    {
                        string methodPrefix = $"[{method.Type}]@{method.SourcePath}";
                        string methodHeader =
                            $"{method.Privacy} {method.ReturnType ?? ""} {method.Name} {method.Parameters ?? "()"}"
                                .Trim();
                        string methodSummary = method.Summary?.Trim() ?? "No summary";
                        string methodReferences = method.ReferencedTypes != null && method.ReferencedTypes.Any()
                            ? string.Join(", ", method.ReferencedTypes.Select(r => r.Name))
                            : "None";

                        lines.Add($"{methodPrefix}: {methodHeader} {methodSummary} References: {methodReferences}");
                    }
                }
            }
        }

        File.WriteAllLines(outputPath, lines);
        Console.WriteLine($"✓ Program summary written to: {outputPath}");
    }
}


/*                                        aass
var ollama = new OllamaClient();

while (true)
{
    Console.Write("You: ");
    var input = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(input)) continue;

    Console.Write("LLM: ");
    await ollama.StreamPromptAsync("gemma3:4b", input);
}
while (true)
{
    Console.Write("You: ");
    var userInput = Console.ReadLine();

    var reply = await ollama.SendPromptAsync("llama3", userInput);

    Console.WriteLine($"LLM: {reply}");
}*/