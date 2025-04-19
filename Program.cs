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

    static async Task Main(string[] args)
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
        WriteGroupedJson(JsonPath, grouped);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✓  Bundled {allItems.Count} items → {JsonPath}\"");
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
        return items.GroupBy(item =>
            {
                var rel = Path.GetRelativePath(rootPath, item.SourcePath).Replace('\\', '/');
                var file = Path.GetFileName(item.SourcePath);
                var dir = Path.GetDirectoryName(rel);
                return $"{file}@{(string.IsNullOrEmpty(dir) ? "." : dir)}";
            })
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    private static void WriteGroupedJson(string outputPath, Dictionary<string, List<TypeRepresentation>> grouped)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        using var writer = new Utf8JsonWriter(File.Create(outputPath), new JsonWriterOptions { Indented = true });
        writer.WriteStartArray();
        foreach (var kvp in grouped)
        {
            writer.WriteStartObject();
            writer.WritePropertyName(kvp.Key);
            JsonSerializer.Serialize(writer, kvp.Value, options);
            writer.WriteEndObject();
        }

        writer.WriteEndArray();
        writer.Flush();
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