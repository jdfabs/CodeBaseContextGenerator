using System.Text.Json;

namespace CodeBaseContextGenerator;

public static class ProjectExplorer
{
    static List<ExplorerNode> LoadFromJson(string path)
    {
        var nodes = new List<ExplorerNode>();

        var raw = File.ReadAllText(path);
        var parsed = JsonSerializer.Deserialize<List<Dictionary<string, List<TypeRepresentation>>>>(raw);
        if (parsed == null) return nodes;

        foreach (var fileGroup in parsed.SelectMany(dict => dict))
        {
            var fileNode = new ExplorerNode
            {
                Label = fileGroup.Key,
                Type = "file",
                Summary = ""
            };

            foreach (var item in fileGroup.Value)
            {
                var typeNode = new ExplorerNode
                {
                    Label = item.Name,
                    Type = item.Type,
                    Summary = item.Summary ?? "No summary",
                    Data = item  // ✅ attach full TypeRepresentation
                };

                if (item.Methods != null)
                {
                    foreach (var method in item.Methods)
                    {
                        typeNode.Children.Add(new ExplorerNode
                        {
                            Label = method.Name,
                            Type = method.Type,
                            Summary = method.Summary ?? "No summary",
                            Data = method  // ✅ attach full method representation
                        });
                    }
                }

                fileNode.Children.Add(typeNode);
            }


            nodes.Add(fileNode);
        }

        return nodes;
    }

    public static void BrowseStructure(string jsonPath)
    {
        var rootNodes = LoadFromJson(jsonPath);
        var flatView = Flatten(rootNodes);
        int index = 0;

        while (true)
        {
            Console.Clear();
            for (int i = 0; i < flatView.Count; i++)
            {
                var node = flatView[i];
                if (i == index) Console.BackgroundColor = ConsoleColor.DarkGray;

                Console.WriteLine($"{new string(' ', node.indent * 2)}[{node.node.Type}] {node.node.Label}");

                if (i == index) Console.ResetColor();
            }

            Console.WriteLine("\nUse W/S to navigate, Enter to expand/collapse, Space to view summary");
            var key = Console.ReadKey(true).Key;

            switch (key)
            {
                case ConsoleKey.W:
                    index = Math.Max(0, index - 1);
                    break;
                case ConsoleKey.S:
                    index = Math.Min(flatView.Count - 1, index + 1);
                    break;
                case ConsoleKey.Enter:
                    flatView[index].node.Expanded = !flatView[index].node.Expanded;
                    flatView = Flatten(rootNodes);
                    break;
                case ConsoleKey.Spacebar:
                    Console.Clear();
                    Console.WriteLine(flatView[index].node.Summary);
                    Console.WriteLine("\nPress any key to return...");
                    Console.ReadKey(true);
                    break;
                case ConsoleKey.Escape:
                    return;
                case ConsoleKey.C:
                {
                    var selected = flatView[index].node;

                    if (selected.Type != "method" || selected.Data == null)
                    {
                        Console.WriteLine("⚠ Please select a method to generate context.");
                        Console.ReadKey(true);
                        break;
                    }

                    var projectRoot = Path.GetFullPath(
                        Path.Combine(Environment.CurrentDirectory, "..", "..", ".."));

                    var fileName = $"{selected.Label.Replace('.', '_')}_context.json";
                    var outPath = Path.Combine(projectRoot, fileName);


                    try
                    {
                        WriteMethodContext("../../../project_context.json", selected, outPath);
                        Console.WriteLine($"✓ Context written to {outPath}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Failed to write context: {ex.Message}");
                    }

                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey(true);
                    break;
                }

            }
        }
    }

    private static List<(ExplorerNode node, int indent)> Flatten(List<ExplorerNode> roots, int depth = 0)
    {
        var result = new List<(ExplorerNode, int)>();

        foreach (var node in roots)
        {
            result.Add((node, depth));
            if (node.Expanded && node.Children.Any())
            {
                result.AddRange(Flatten(node.Children, depth + 1));
            }
        }

        return result;
    }
    
    private static void WriteMethodContext(
        string jsonIn,
        ExplorerNode method,
        string jsonOut)
    {
        // 1) Load full structure
        var allGroups = JsonSerializer.Deserialize<
                List<Dictionary<string, List<TypeRepresentation>>>>(
                File.ReadAllText(jsonIn)
            )
            .SelectMany(d => d)
            .ToDictionary(k => k.Key, v => v.Value);

        // 2) build lookup
        var lookup = allGroups
            .SelectMany(g => g.Value)
            .SelectMany(t =>
            {
                var items = new List<TypeRepresentation>();
                if (t != null)
                {
                    items.Add(t);
                    if (t.Methods != null)
                        items.AddRange(t.Methods.Where(m => m != null));
                }
                return items;
            })
            .Where(tr => tr != null && !string.IsNullOrWhiteSpace(tr.Name) && !string.IsNullOrWhiteSpace(tr.SourcePath))
            .ToDictionary(
                tr => $"{tr.Name}@{tr.SourcePath}",
                tr => tr
            );


        // 3) find the method
        var methodRep = lookup.Values.FirstOrDefault(m =>
            m.Type == "method" &&
            m.Name == method.Data.Name &&
            m.SourcePath == method.Data.SourcePath);

        if (methodRep == null)
            throw new InvalidOperationException(
                $"Method not found: {method.Data.Name}@{method.Data.SourcePath}");

        // 4) declaring type
        var className     = methodRep.Name.Split('.')[0];
        var declaringKey  = $"{className}@{methodRep.SourcePath}";
        lookup.TryGetValue(declaringKey, out var declaringRep);

        // 5) base types
        var baseTypes = (declaringRep?.ReferencedTypes ?? Enumerable.Empty<TypeReference>())
            .Where(r => r.Kind is "extends" or "implements")
            .Select(r => lookup.TryGetValue(r.Source, out var rep) ? rep : null)
            .Where(r => r != null)
            .ToList();

        // 6) used types
        var usedTypes = methodRep.ReferencedTypes
            .Where(r => r.Kind is "uses" or "throws")
            .Select(r => lookup.TryGetValue(r.Source, out var rep) ? rep : null)
            .Where(r => r != null)
            .ToList();

        // 7) assemble context
        var context = new
        {
            Method        = methodRep,
            DeclaringType = declaringRep,
            BaseTypes     = baseTypes,
            UsedTypes     = usedTypes
        };

        // 8) write out
        var options = new JsonSerializerOptions { WriteIndented = true };
        File.WriteAllText(jsonOut, JsonSerializer.Serialize(context, options));
    }

}

class ExplorerNode
{
    public string Label { get; set; }         // "SocketConnection.sendSecure"
    public string Type { get; set; }          // "method", "class", etc.
    public string Summary { get; set; }
    public List<ExplorerNode> Children { get; set; } = new();
    public bool Expanded { get; set; } = false;

    // ✅ Reference to original object
    public TypeRepresentation Data { get; set; } 
}
