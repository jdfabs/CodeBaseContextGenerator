using System.Text.Json;
using System.Text.Json.Serialization;

namespace CodeBaseContextGenerator.Core.Utils.Json;

public static class JsonWriter
{
    private static readonly JsonSerializerOptions _options = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static void SaveMerged(string outputPath, List<Dictionary<string, List<TypeRepresentation>>> merged)
    {
        try
        {
            var json = JsonSerializer.Serialize(merged, _options);
            File.WriteAllText(outputPath, json);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"✓ JSON written to: {outputPath}");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ Failed to write JSON file: {outputPath}");
            Console.WriteLine(ex.Message);
        }
        finally
        {
            Console.ResetColor();
        }
    }
}