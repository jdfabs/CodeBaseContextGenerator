using System.Text.Json;
using CodeBaseContextGenerator.Cli.Models;

namespace CodeBaseContextGenerator.Cli.Project;

public class ProjectModelLoader
{
    public static List<ExplorerNode> Load(string path)
    {
        if (!File.Exists(path))
        {
            Console.WriteLine($"❌ JSON file not found: {Path.GetFullPath(path)}");
            return new();
        }

        var raw = File.ReadAllText(path);
        if (string.IsNullOrWhiteSpace(raw))
        {
            Console.WriteLine($"⚠ JSON file is empty: {Path.GetFullPath(path)}");
            return new();
        }

        try
        {
            var parsed = JsonSerializer.Deserialize<List<Dictionary<string, List<TypeRepresentation>>>>(raw);
            return BuildTree(parsed ?? []);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to parse JSON: {path}");
            Console.WriteLine(ex.Message);
            return new();
        }
    }

    private static List<ExplorerNode> BuildTree(List<Dictionary<string, List<TypeRepresentation>>> parsed)
    {
        var result = new List<ExplorerNode>();
        foreach (var fileGroup in parsed.SelectMany(d => d))
        {
            var fileNode = new ExplorerNode { Label = fileGroup.Key, Type = "file" };

            foreach (var type in fileGroup.Value)
            {
                var typeNode = new ExplorerNode();
                typeNode.Label = type.Name;
                typeNode.Type = type.Type;
                typeNode.Summary = type.Summary;
                typeNode.Data = type;

                foreach (var method in type.Methods)
                {
                    typeNode.Children.Add(new ExplorerNode
                    {
                        Label = method.Name,
                        Type = method.Type,
                        Summary = method.Summary, 
                        Data = method
                    });
                }

                fileNode.Children.Add(typeNode);
            }

            result.Add(fileNode);
        }
        return result;
    }
}
