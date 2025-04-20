using CodeBaseContextGenerator.Cli.Models;

namespace CodeBaseContextGenerator.Cli.ProjectExplorer;

public static class ExplorerTreeFlattener
{
    public static List<ExplorerNode> Flatten(List<ExplorerNode> roots, int depth = 0)
    {
        var result = new List<ExplorerNode>();

        foreach (var node in roots)
        {
            node.Indent = depth;
            result.Add(node);

            if (node.Expanded && node.Children?.Count > 0)
                result.AddRange(Flatten(node.Children, depth + 1));
        }
        return result;
    }
}
