using System.Text.Json;
using System.Text.Json.Serialization;
using Antlr4.Runtime;
using CodeBaseContextGenerator.JavaAntlr4;

namespace CodeBaseContextGenerator;

class Program
{
    private static string jsonPath = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName, "project_context.json");
    private static string path;

    static async Task Main(string[] args)
    {
        path = FileExplorer.Browse();

        InspectJavaPath(path);
        while (true)
        {
            Console.WriteLine("Press any key to reload files");
            Console.ReadKey();
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
    }
    
    private static void InspectJavaPath(string path)
    {
        var allItems = new List<TypeRepresentation>();
        string rootPath = File.Exists(path) ? Path.GetDirectoryName(path) : path;

        if (File.Exists(path) && Path.GetExtension(path) == ".java")
        {
            allItems.AddRange(InspectSingleJavaFile(path, rootPath));
        }
        else if (Directory.Exists(path))
        {
            var javaFiles = Directory.GetFiles(path, "*.java", SearchOption.AllDirectories);
            foreach (var file in javaFiles)
            {
                allItems.AddRange(InspectSingleJavaFile(file, rootPath));
            }
        }
        else
        {
            Console.WriteLine("Invalid path. Must be a .java file or folder.");
            return;
        }

        ResolveTypeReferences(allItems);

        var grouped = GroupByFile(allItems, rootPath);

        WriteGroupedJson(jsonPath, grouped);
        Console.WriteLine($"✓ Bundled and saved to {jsonPath}");
    }




    private static List<TypeRepresentation> InspectSingleJavaFile(string filePath, string rootPath)
    {
        var code = File.ReadAllText(filePath);
        var input = new AntlrInputStream(code);
        var lexer = new JavaLexer(input);
        var tokens = new CommonTokenStream(lexer);
        var parser = new JavaParser(tokens) { BuildParseTree = true };

        var tree = parser.compilationUnit();
        var inspector = new JavaClassInspector(filePath, rootPath);

        return inspector.Visit(tree);
    }
    
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
                        Console.WriteLine($" ↳ {entry.SourcePath}");
                }
                return g.First();
            });

        foreach (var type in allItems)
        {
            // ✅ First resolve this type's own references (e.g., implements/extends)
            if (type.ReferencedTypes != null)
            {
                foreach (var reference in type.ReferencedTypes)
                {
                    if (allTypesByName.TryGetValue(reference.Name, out var targetType))
                    {
                        reference.Source = $"{reference.Name}@{targetType.SourcePath}";
                    }
                    else
                    {
                        Console.WriteLine($"[Warning] Unresolved type reference (type): {reference.Name} in {type.Name}");
                    }
                }
            }

            // ✅ Then resolve all of its methods' references
            if (type.Methods == null) continue;

            foreach (var method in type.Methods)
            {
                if (method.ReferencedTypes == null) continue;

                foreach (var reference in method.ReferencedTypes)
                {
                    if (allTypesByName.TryGetValue(reference.Name, out var targetType))
                    {
                        reference.Source = $"{reference.Name}@{targetType.SourcePath}";
                    }
                    else
                    {
                        Console.WriteLine($"[Warning] Unresolved type reference (method): {reference.Name} in {method.Name}");
                    }
                }
            }
        }
    }
    private static Dictionary<string, List<TypeRepresentation>> GroupByFile(List<TypeRepresentation> items, string rootPath)
    {
        var groups = new Dictionary<string, List<TypeRepresentation>>();

        foreach (var item in items)
        {
            var fileName = Path.GetFileName(item.SourcePath);
            var relPath = Path.GetDirectoryName(item.SourcePath)?.Replace('\\', '/'); // cross-platform friendly
            if (string.IsNullOrEmpty(relPath) || relPath == ".") relPath = ".";

            var key = $"{fileName}@{relPath}";

            if (!groups.ContainsKey(key))
                groups[key] = new List<TypeRepresentation>();

            groups[key].Add(item);
        }

        return groups;
    }

    private static void WriteGroupedJson(string outputPath, Dictionary<string, List<TypeRepresentation>> grouped)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        using var stream = File.CreateText(outputPath);
        using var writer = new Utf8JsonWriter(stream.BaseStream, new JsonWriterOptions { Indented = true });

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