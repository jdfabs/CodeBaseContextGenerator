using CodeBaseContextGenerator.Cli.Models;
using CodeBaseContextGenerator.Json;

namespace CodeBaseContextGenerator.Cli.Project;

public class ProjectModelLoader
{
    public static List<ExplorerNode> Load(string path)
    {
        var parsed = JsonLoader.Load<List<Dictionary<string, List<TypeRepresentation>>>>(path);
        if (parsed == null) return new();

        return BuildTree(parsed);
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
