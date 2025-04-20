using CodeBaseContextGenerator.Cli.Models;

namespace CodeBaseContextGenerator.Cli.Project;

public class SummaryAggregator
{
    public static List<(string title, string summary)> Collect(ExplorerNode node)
    {
        var list = new List<(string, string)>();

        void Walk(ExplorerNode n)
        {
            if (!string.IsNullOrWhiteSpace(n.Summary))
                list.Add(($"[{n.Type}] {n.Label}", n.Summary));

            foreach (var child in n.Children ?? Enumerable.Empty<ExplorerNode>())
                Walk(child);
        }

        Walk(node);
        return list;
    }
}