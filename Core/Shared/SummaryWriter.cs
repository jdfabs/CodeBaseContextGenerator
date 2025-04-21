namespace CodeBaseContextGenerator.Core.Shared;

public static class SummaryWriter
{
    public static void Write(string outputPath, List<Dictionary<string, List<TypeRepresentation>>> data)
    {
        try
        {
            var lines = GenerateLines(data);
            File.WriteAllLines(outputPath, lines);
            Console.WriteLine($"✓ Summary written to: {outputPath}", ConsoleColor.Green);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Failed to write summary: {outputPath}\n{ex.Message}", ConsoleColor.Red);
        }
    }

    private static List<string> GenerateLines(List<Dictionary<string, List<TypeRepresentation>>> data)
    {
        return data
            .SelectMany(dict => dict)
            .SelectMany(kvp => kvp.Value.SelectMany(type =>
            {
                var lines = new List<string> { Format(type, kvp.Key) };
                if (type.Methods != null)
                    lines.AddRange(type.Methods.Select(m => Format(m, kvp.Key)));
                return lines;
            }))
            .ToList();
    }

    private static string Format(TypeRepresentation item, string fileKey)
    {
        var header = $"{item.Privacy} {item.ReturnType ?? ""} {item.Name} {item.Parameters ?? "()"}".Trim();
        var summary = item.Summary?.Trim() ?? "No summary";
        var refs = item.ReferencedTypes?.Any() == true
            ? string.Join(", ", item.ReferencedTypes.Select(r => r.Name))
            : "None";

        return $"[{item.Type}]@{fileKey}: {header} {summary} References: {refs}";
    }
}
