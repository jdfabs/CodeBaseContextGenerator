namespace CodeBaseContextGenerator.Core;

public static class SummaryWriter
{
    public static void Write(string outputPath, List<Dictionary<string, List<TypeRepresentation>>> data)
    {
        try
        {
            var lines = new List<string>();

            foreach (var fileGroup in data.SelectMany(dict => dict))
            {
                var filePath = fileGroup.Key;

                foreach (var type in fileGroup.Value)
                {
                    string typeLine = Format(type, filePath);
                    lines.Add(typeLine);

                    if (type.Methods != null)
                    {
                        foreach (var method in type.Methods)
                            lines.Add(Format(method, filePath));
                    }
                }
            }

            File.WriteAllLines(outputPath, lines);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ Summary written to: {outputPath}");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ Failed to write summary: {outputPath}");
            Console.WriteLine(ex.Message);
        }
        finally
        {
            Console.ResetColor();
        }
    }

    private static string Format(TypeRepresentation item, string fileKey)
    {
        string header = $"{item.Privacy} {item.ReturnType ?? ""} {item.Name} {item.Parameters ?? "()"}".Trim();
        string summary = item.Summary?.Trim() ?? "No summary";
        string refs = item.ReferencedTypes != null && item.ReferencedTypes.Any()
            ? string.Join(", ", item.ReferencedTypes.Select(r => r.Name))
            : "None";

        return $"[{item.Type}]@{fileKey}: {header} {summary} References: {refs}";
    }
}