using System.Text.Json;
using System.Text.RegularExpressions;

namespace CodeBaseContextGenerator;

public class TypeUtils
{
    private static readonly string HashFile = Path.Combine(
        Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName,
        "hashes.json"
    );
    
    public static List<TypeRepresentation> ExtractJavaTypes(string source)
    {
        var result = new List<TypeRepresentation>();

        var pattern =
            @"(public\s+|private\s+|protected\s+)?(abstract\s+|final\s+)?(class|interface|enum|record)\s+(\w+)\s*[^;{]*\{";
        var matches = Regex.Matches(source, pattern);

        foreach (Match match in matches)
        {
            var kind = match.Groups[3].Value;
            var name = match.Groups[4].Value;
            var startIndex = match.Index;
            var body = ExtractFullTypeBody(source, startIndex);

            result.Add(new TypeRepresentation
            {
                Name = name,
                Type = kind,
                Body = body
            });
        }

        return result;
    }

    public static string ExtractFullTypeBody(string source, int startIndex)
    {
        int braceCount = 0;
        int i = startIndex;
        while (i < source.Length)
        {
            if (source[i] == '{') braceCount++;
            else if (source[i] == '}') braceCount--;

            i++;

            if (braceCount == 0 && i > startIndex) break;
        }

        return source.Substring(startIndex, i - startIndex);
    }

    public static Dictionary<string, string> LoadPreviousHashes()
    {
        if (!File.Exists(HashFile)) return new Dictionary<string, string>();

        var json = File.ReadAllText(HashFile);
        return JsonSerializer.Deserialize<Dictionary<string, string>>(json) ?? new();
    }

    public static void SaveHashes(Dictionary<string, string> hashes)
    {
        Console.WriteLine($"💾 Saving hashes to: {HashFile}");

        var json = JsonSerializer.Serialize(hashes, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(HashFile, json);
    }
}