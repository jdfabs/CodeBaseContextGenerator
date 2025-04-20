using CodeBaseContextGenerator.Cli.Models;

namespace CodeBaseContextGenerator.Cli;

public class FileExplorer : ExplorerUI<FileNode>
{
    private DirectoryInfo _currentDirectory;
    private string _lastVisitedFolderName = string.Empty;

    public string SelectedPath { get; private set; } = string.Empty;
    private List<string> Extensions { get; }

    public FileExplorer(params string[] extensions)
    {
        _currentDirectory = new DirectoryInfo(
            Directory.GetParent(Environment.CurrentDirectory)?.Parent?.Parent?.FullName
            ?? Environment.CurrentDirectory
        );

        Extensions = extensions.ToList();
    }

    public string BrowseAndGetPath()
    {
        Browse();
        return SelectedPath;
    }

    protected override string CurrentPath => _currentDirectory.FullName;

    protected override List<FileNode> LoadCurrentLevel()
    {
        var dirs = _currentDirectory.GetDirectories();

        var files = Extensions
            .SelectMany(ext =>
            {
                var cleanExt = ext.Replace(".", "").Replace(" ", "");
                return _currentDirectory.GetFiles($"*.{cleanExt}");
            })
            .ToList();

        return dirs
            .Cast<FileSystemInfo>()
            .Concat(files)
            .OrderBy(entry => entry.Name)
            .Select(entry => new FileNode
            {
                Label = entry.Name,
                Type = entry is DirectoryInfo ? "folder" : "file",
                Data = entry
            })
            .ToList();
    }

    protected override void RenderNode(FileNode node, bool isSelected)
    {
        if (isSelected)
            Console.BackgroundColor = HighlightColor;

        Console.WriteLine($"{node.Icon} {node.Label}");

        if (isSelected)
            Console.ResetColor();
    }

    protected override bool Raise()
    {
        if (_currentDirectory.Parent == null)
            return false;

        _lastVisitedFolderName = _currentDirectory.Name;
        LastSelectedLabel = null;
        _currentDirectory = _currentDirectory.Parent;

        return true;
    }

    protected override bool Lower(FileNode node)
    {
        if (node.Type != "folder" || node.Data is not DirectoryInfo dir)
            return false;

        _currentDirectory = dir;
        _lastVisitedFolderName = null;
        LastSelectedLabel = string.Empty;

        return true;
    }

    protected override bool Confirm(FileNode node)
    {
        SelectedPath = node.Data.FullName;
        return true;
    }

    protected override void OnReload()
    {
        if (Nodes == null || Nodes.Count == 0)
        {
            _lastVisitedFolderName = null;
            return;
        }

        if (!string.IsNullOrEmpty(_lastVisitedFolderName))
        {
            int idx = Nodes.FindIndex(n => n.Label == _lastVisitedFolderName);
            if (idx >= 0)
                SetSelectedIndex(idx);

            _lastVisitedFolderName = null;
        }
        else
        {
            SetSelectedIndex(0);
        }
    }
}