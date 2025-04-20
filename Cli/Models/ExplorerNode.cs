namespace CodeBaseContextGenerator.Cli.Models;

public class ExplorerNode : BaseNode
{
    public string Summary { get; set; } = "No Summary";
    public string Type { get; set; } = "No Type"; // "Class", "Interface", "Enum", "Struct", "Record", "File"
    public List<ExplorerNode> Children { get; set; } = [];
    public bool Expanded { get; set; }
    public TypeRepresentation Data { get; set; }

    public int Indent { get; set; } = 0;
    public override string Icon => Type switch
    {
        "class" => "\ud83d\udcda",
        "interface" => "\ud83e\udd1d",
        "enum" => "📦",
        "struct" => "\ud83d\udcbc",
        "record" =>"\ud83d\udcbed", 
        "file" => "📄",
        "method" => "\u2699\ufe0f",
        _ => "❓"
    };
}