namespace CodeBaseContextGenerator.Json;

public class JsonDiffResult
{
    public int ChangeCount { get; set; }
    public bool HasChanges => ChangeCount > 0;

    public List<string> RemovedFiles { get; set; } = new();
    public List<string> RemovedTypes { get; set; } = new();
    public List<string> RemovedMethods { get; set; } = new();

    public List<string> ChangedTypes { get; set; } = new();
    public List<string> ChangedMethods { get; set; } = new();

    public List<Dictionary<string, List<TypeRepresentation>>> FinalStructure { get; set; } = new();

    public void LogSummary()
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n=== 🧠 Diff Summary ===");

        if (RemovedFiles.Any())
            Console.WriteLine($"✖ Removed Files: {string.Join(", ", RemovedFiles)}");

        if (RemovedTypes.Any())
            Console.WriteLine($"✖ Removed Types: {string.Join(", ", RemovedTypes)}");

        if (RemovedMethods.Any())
            Console.WriteLine($"✖ Removed Methods: {string.Join(", ", RemovedMethods)}");

        if (ChangedTypes.Any())
            Console.WriteLine($"✴ Modified Types: {string.Join(", ", ChangedTypes)}");

        if (ChangedMethods.Any())
            Console.WriteLine($"✴ Modified Methods: {string.Join(", ", ChangedMethods)}");

        if (!HasChanges)
            Console.WriteLine("✓ No changes detected.");

        Console.ResetColor();
    }
}