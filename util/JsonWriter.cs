using System.Text.Json;
using System.Text.Json.Serialization;
using CodeBaseContextGenerator;

public static class JsonWriter
{
    public static void WriteToFile(string outputPath, List<TypeRepresentation> types)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        var json = JsonSerializer.Serialize(types, options);
        File.WriteAllText(outputPath, json);
    }
}