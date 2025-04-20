namespace CodeBaseContextGenerator.Core;

public class ChangeResult
{
    public bool HasChanges { get; init; }
    public int ChangeCount { get; init; }
    public List<Dictionary<string, List<TypeRepresentation>>> MergedJson { get; init; } = [];

    public static readonly ChangeResult NoChanges = new()
    {
        HasChanges = false,
        ChangeCount = 0,
        MergedJson = []
    };
}