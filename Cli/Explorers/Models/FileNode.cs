namespace CodeBaseContextGenerator.Cli.Models;

public class FileNode : BaseNode
{
    public string Type { get; set; } = "No Type"; // "folder" or "file"

    public FileSystemInfo Data { get; set; }
    public override string Icon => Data is DirectoryInfo ? "📂" : "📄";
}