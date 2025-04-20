namespace CodeBaseContextGenerator.Core;

public class ChangeResult
{
    public bool HasChanges { get; set; }
    public int ChangeCount { get; set; }
    public List<Dictionary<string, List<TypeRepresentation>>> MergedJson { get; set; } = new();
}