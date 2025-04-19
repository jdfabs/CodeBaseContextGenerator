using System.Text.Json;
using System.Text.RegularExpressions;

namespace CodeBaseContextGenerator;

public class TypeUtils
{
    private static readonly string HashFile = Path.Combine(
        Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName,
        "hashes.json"
    );

    public static List<TypeRepresentation> ExtractJavaTypes(string source, string filePath)
    {
        var result = new List<TypeRepresentation>();

        var pattern = @"(?<privacy>public|private|protected)?\s*(?<modifier>abstract|final)?\s*(?<type>class|interface|enum|record)\s+(?<name>\w+)\s*(?<params>\([^)]*\))?\s*[^;{]*\{";
        var matches = Regex.Matches(source, pattern);

        foreach (Match match in matches)
        {
            var kind = match.Groups["type"].Value;
            var name = match.Groups["name"].Value;
            var privacy = match.Groups["privacy"].Success ? match.Groups["privacy"].Value : "default";
            var parameters = match.Groups["params"].Success ? match.Groups["params"].Value : "";
            var startIndex = match.Index;
            var content = ExtractFullTypeBody(source, startIndex);
            var relativePath = Path.GetRelativePath(Directory.GetCurrentDirectory(), filePath);

            var type = new TypeRepresentation
            {
                Name = name,
                Type = kind,
                Privacy = privacy,
                Parameters = parameters,
                Content = content,
                ReturnType = null,
                SourcePath = relativePath
            };

            result.Add(type);

            // Extract methods within the type
            result.AddRange(ExtractMethods(type));
        }

        return result;
    }

    private static List<TypeRepresentation> ExtractMethods(TypeRepresentation parentType)
    {
        var results = new List<TypeRepresentation>();

        // Matches methods like: public static int add(int x, int y) {
        var methodPattern = @"(?<privacy>public|private|protected)?\s*(static\s*)?(?<return>\w+)\s+(?<name>\w+)\s*\((?<params>[^)]*)\)\s*\{";

        var matches = Regex.Matches(parentType.Content, methodPattern);

        foreach (Match match in matches)
        {
            var methodName = match.Groups["name"].Value;
            var returnType = match.Groups["return"].Value;
            var parameters = match.Groups["params"].Value;
            var privacy = match.Groups["privacy"].Success ? match.Groups["privacy"].Value : "default";
            var startIndex = match.Index;

            var methodBody = TypeUtils.ExtractFullTypeBody(parentType.Content, startIndex);

            results.Add(new TypeRepresentation
            {
                Name = methodName,
                Type = "method",
                ReturnType = returnType,
                Privacy = privacy,
                Parameters = parameters,
                Content = methodBody,
                SourcePath = parentType.SourcePath
            });
        }

        return results;
    }
public static void PopulateReferencedTypes(List<TypeRepresentation> allTypes)
{
    var knownTypes = allTypes.ToDictionary(
        t => $"{t.Name}@{t.SourcePath}",
        t => t
    );

    foreach (var type in allTypes)
    {
        var references = new List<TypeReference>();
        var content = type.Content;

        foreach (var candidate in allTypes)
        {
            if (candidate.Name == type.Name && candidate.SourcePath == type.SourcePath)
                continue;

            var refKey = $"{candidate.Name}@{candidate.SourcePath}";

            bool AlreadyReferenced(string kind) =>
                references.Any(r => r.Name == candidate.Name && r.Kind == kind);

            void Add(string kind)
            {
                if (!AlreadyReferenced(kind))
                {
                    references.Add(new TypeReference
                    {
                        Name = candidate.Name,
                        Source = refKey,
                        Kind = kind
                    });
                }
            }

            // EXTENDS
            if (Regex.IsMatch(content, $@"\bextends\s+{Regex.Escape(candidate.Name)}\b"))
            {
                Add("extends");
                Add("superclass"); // alias
                continue; // don't fall through to 'uses'
            }

            // IMPLEMENTS
            if (Regex.IsMatch(content, $@"\bimplements\s+[^{{;\n]*\b{Regex.Escape(candidate.Name)}\b"))
            {
                Add("implements");
                continue; // don't fall through to 'uses'
            }

            // THROWS
            if (Regex.IsMatch(content, $@"\bthrows\s+[^{{;\n]*\b{Regex.Escape(candidate.Name)}\b"))
                Add("throws");

            // IMPORTS
            if (Regex.IsMatch(content, $@"\bimport\s+[^\n;]*\b{Regex.Escape(candidate.Name)}\b"))
                Add("imports");

            // ANNOTATION
            if (Regex.IsMatch(content, $@"@{Regex.Escape(candidate.Name)}\b"))
                Add("annotation");

            // USES — only if not already accounted for via extends/implements
            if (!AlreadyReferenced("extends") && !AlreadyReferenced("implements"))
            {
                if (Regex.IsMatch(content, $@"\b{Regex.Escape(candidate.Name)}\b"))
                    Add("uses");
            }
        }

        type.ReferencedTypes = references;
    }
}


    public static string ExtractFullTypeBody(string source, int startIndex)
    {
        int braceCount = 0;
        int i = startIndex;
        bool inside = false;

        while (i < source.Length)
        {
            if (source[i] == '{')
            {
                braceCount++;
                inside = true;
            }
            else if (source[i] == '}')
            {
                braceCount--;
            }

            i++;

            if (inside && braceCount == 0)
            {
                break;
            }
        }

        // Ensure valid slice
        if (i > source.Length) i = source.Length;

        return source.Substring(startIndex, i - startIndex);
    }


    public static Dictionary<string, TypeHashEntry> LoadPreviousHashes()
    {
        if (!File.Exists(HashFile)) return new();

        var json = File.ReadAllText(HashFile);
        return JsonSerializer.Deserialize<Dictionary<string, TypeHashEntry>>(json) ?? new();
    }

    public static void SaveHashes(Dictionary<string, TypeHashEntry> hashes)
    {
        Console.WriteLine($"💾 Saving hashes to: {HashFile}");

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };
        File.WriteAllText(HashFile, JsonSerializer.Serialize(hashes, options));
    }
}