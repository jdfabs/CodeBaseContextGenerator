namespace CodeBaseContextGenerator.Cli.Models;

public abstract class BaseNode
{
    public string Label { get; set; } = "Default Label";
    public abstract string Icon { get; }
}