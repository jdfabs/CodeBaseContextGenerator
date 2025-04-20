using CodeBaseContextGenerator.Cli.Models;

namespace CodeBaseContextGenerator.Cli.ProjectExplorer;

public class ProjectExplorer(string jsonPath) : ExplorerUI<ExplorerNode>
{
    private readonly List<ExplorerNode> _rootNodes = ProjectModelLoader.Load(jsonPath);

    protected override List<ExplorerNode> LoadCurrentLevel()
        => ExplorerTreeFlattener.Flatten(_rootNodes);

    protected override void RenderNode(ExplorerNode node, bool isSelected)
    {
        if (isSelected) Console.BackgroundColor = HighlightColor;
        Console.WriteLine($"{new string(' ', node.Indent * 2)}{node.Icon}[{node.Type.ToLower()}] {node.Label}");
        if (isSelected) Console.ResetColor();
    }

    protected override bool Lower(ExplorerNode node)
    {
        if (!(node.Children.Count > 0) || node.Expanded) return false;
        
        node.Expanded = true;
        return true;
    }

    protected override bool Raise()
    {
        if (Nodes.Count == 0) return false;

        var current = GetCurrent();

        if (current.Expanded)
        {
            current.Expanded = false;
            return true;
        }

        if (current.Type == "method")
        {
            var parent = FindParent(_rootNodes, current);
            return CollapseIfFound(parent);
        }

        if (IsTypeNode(current.Type))
        {
            var parent = FindParent(_rootNodes, current);
            return CollapseIfFound(parent?.Type == "file" ? parent : null);
        }

        return false;
    }

    protected override bool Confirm(ExplorerNode node)
    {
        Console.Clear();

        if (node.Type == "method")
        {
            Console.WriteLine($"📄 Summary for `{node.Label}`:\n");
            Console.WriteLine(node.Summary);
        }
        else
        {
            Console.WriteLine($"📦 Bundled Summary for `{node.Label}` ({node.Type}):\n");
            foreach (var (title, summary) in GatherSummaries(node))
            {
                Console.WriteLine($"➤ {title}");
                Console.WriteLine($"{summary}\n");
            }
        }

        Console.WriteLine("Press any key to continue...");
        Console.ReadKey(true);
        return false;
    }

    protected override void OnBeforeKey(ConsoleKey key)
    {
        if (key == ConsoleKey.C && Nodes.Count > 0)
        {
            var node = GetCurrent();
            TryExportContext(node);
        }
    }

    // ─── Helpers ───────────────────────────────────────────────────────

    private static List<(string title, string summary)> GatherSummaries(ExplorerNode node)
    {
        return SummaryAggregator.Collect(node); 
    }

    private bool IsTypeNode(string type) =>
        type is "class" or "interface" or "record" or "struct" or "enum";

    private ExplorerNode? FindParent(IEnumerable<ExplorerNode> roots, ExplorerNode target)
    {
        foreach (var node in roots)
        {
            if (node.Children.Contains(target)) return node;
            var result = FindParent(node.Children, target);
            if (result != null) return result;
        }
        return null;
    }

    private bool CollapseIfFound(ExplorerNode? parent)
    {
        if (parent is not { Expanded: true }) return false;
        
        parent.Expanded = false;
        SetSelectedToNode(parent);
        return true;
    }

    private void SetSelectedToNode(ExplorerNode target) => SetSelectedIndex(Nodes.IndexOf(target));

    private void TryExportContext(ExplorerNode node)
    {
        if (node.Type != "method" || node.Data is null)
        {
            Console.WriteLine("⚠ Please select a method to generate context.");
            Console.ReadKey(true);
            return;
        }

        var fileName = $"{node.Label.Replace('.', '_')}_context.json";
        var projectRoot = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, "..", "..", ".."));
        var outPath = Path.Combine(projectRoot, fileName);

        try
        {
            Console.WriteLine($"🔍 Writing method context for: {node.Label}");
            Console.WriteLine($"  ➜ Output path: {outPath}");
            ContextExporter.Export(jsonPath, node, outPath);
            Console.WriteLine($"✓ Context written to {outPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Failed to write context:");
            Console.WriteLine(ex);
        }

        Console.WriteLine("Press any key to continue...");
        Console.ReadKey(true);
    }
}