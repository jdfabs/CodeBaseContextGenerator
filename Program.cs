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

    private static string _chosenPath = string.Empty;

    static async System.Threading.Tasks.Task Main(string[] args)
    {
        // 1️⃣ Let the user pick a .java file or a folder (WASD file‑explorer)
        _chosenPath = FileExplorer.Browse();

        // 2️⃣ Parse + emit JSON (blocking). Hit any key to re‑run.
        InspectJavaPath(_chosenPath);
        while (true)
        {
            Console.WriteLine("Press any key to re‑load & re‑analyze …");
            Console.ReadKey(true);
            InspectJavaPath(_chosenPath);
        }
    }

    /*─────────────────────────  Core  ─────────────────────────*/

    private static void InspectJavaPath(string path)
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
        if ((changeCount = UpdateJsonIfChanged(JsonPath, grouped)) == 0)
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

    private static int UpdateJsonIfChanged(string outputPath, Dictionary<string, List<TypeRepresentation>> grouped)
    {
        int changeCount = 0;
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        // Load existing file
        Dictionary<string, List<TypeRepresentation>>? existing = null;
        if (File.Exists(outputPath))
        {
            try
            {
                var raw = File.ReadAllText(outputPath);
                existing = JsonSerializer.Deserialize<List<Dictionary<string, List<TypeRepresentation>>>>(raw)?
                    .SelectMany(dict => dict)
                    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            }
            catch
            {
                Console.WriteLine("⚠ Could not parse existing JSON. Overwriting everything.");
            }
        }

        var finalOutput = new List<Dictionary<string, List<TypeRepresentation>>>();
        bool anyChanges = false;

        foreach (var kvp in grouped)
        {
            string key = kvp.Key;
            var newItems = kvp.Value;

            var updatedItems = new List<TypeRepresentation>();

            var oldItems = existing != null && existing.TryGetValue(key, out var found)
                ? found
                : new List<TypeRepresentation>();

            foreach (var newItem in newItems)
            {
                var matchingOld = oldItems.FirstOrDefault(o => o.Name == newItem.Name && o.Type == newItem.Type);

                // Check top-level class/interface hash
                bool typeChanged = matchingOld == null || matchingOld.Hash != newItem.Hash;

                // Default to using the new version
                var result = newItem;

                if (!typeChanged && matchingOld?.Methods != null && newItem.Methods != null)
                {
                    var updatedMethods = new List<TypeRepresentation>();
                    bool methodChanged = false;

                    foreach (var newMethod in newItem.Methods)
                    {
                        var matchingOldMethod = matchingOld.Methods
                            .FirstOrDefault(m => m.Name == newMethod.Name && m.Type == "method");

                        if (matchingOldMethod != null && matchingOldMethod.Hash == newMethod.Hash)
                        {
                            updatedMethods.Add(matchingOldMethod);
                            Console.WriteLine($"  ✓ No changes in method {newMethod.Name}");
                        }
                        else
                        {
                            updatedMethods.Add(newMethod);
                            Console.WriteLine($"  ✖ Method changed: {newMethod.Name}");
                            methodChanged = true;
                            changeCount++;
                        }
                    }

                    if (!methodChanged)
                    {
                        Console.WriteLine($"✓ No changes in {newItem.Name} ({newItem.Type})");
                        result = matchingOld;
                    }
                    else
                    {
                        Console.WriteLine($"✖ Changes detected in methods of {newItem.Name} ({newItem.Type})");
                        result.Methods = updatedMethods;
                        anyChanges = true;
                        changeCount++;
                    }
                }
                else if (typeChanged)
                {
                    Console.WriteLine($"✖ Structural change: {newItem.Name} ({newItem.Type})");
                    anyChanges = true;
                    changeCount++;
                }
                else
                {
                    // Perfect match — reuse everything
                    Console.WriteLine($"✓ No changes in {newItem.Name} ({newItem.Type})");
                    result = matchingOld!;
                }

                updatedItems.Add(result);
            }

            finalOutput.Add(new Dictionary<string, List<TypeRepresentation>> { [key] = updatedItems });
        }

        if (!anyChanges)
        {
            return changeCount;
        }

        var finalJson = JsonSerializer.Serialize(finalOutput, options);
        File.WriteAllText(outputPath, finalJson);
        Console.WriteLine("✓ JSON file updated with modified elements.");
        return changeCount;
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