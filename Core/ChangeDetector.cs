using System.Text.Json;
using CodeBaseContextGenerator.LLM;

namespace CodeBaseContextGenerator.Core;

public static class ChangeDetector
{
    public static async Task<ChangeResult> DetectChangesAsync(
        string existingJsonPath,
        List<TypeRepresentation> newItems,
        OllamaClient ollama)
    {
        var groupedNew = GroupByFile(newItems);
        var existing = LoadExisting(existingJsonPath);

        var changeCount = 0;
        var finalOutput = new List<Dictionary<string, List<TypeRepresentation>>>();

        foreach (var kvp in groupedNew)
        {
            var fileKey = kvp.Key;
            var newTypes = kvp.Value;
            existing.TryGetValue(fileKey, out var oldTypes);
            oldTypes ??= new();

            var updated = new List<TypeRepresentation>();

            foreach (var newType in newTypes)
            {
                var oldType = oldTypes.FirstOrDefault(t => t.Name == newType.Name && t.Type == newType.Type);

                if (oldType?.Hash == newType.Hash)
                {
                    updated.Add(oldType); // unchanged
                }
                else
                {
                    // Structural change — generate new summaries
                    newType.Summary = await SummaryGenerator.SummarizeAsync(newType.Code, ollama);

                    var mergedMethods = new List<TypeRepresentation>();

                    if (newType.Methods != null)
                    {
                        foreach (var method in newType.Methods)
                        {
                            var oldMethod =
                                oldType?.Methods?.FirstOrDefault(m => m.Name == method.Name && m.Type == "method");

                            if (oldMethod?.Hash == method.Hash)
                            {
                                mergedMethods.Add(oldMethod);
                            }
                            else
                            {
                                method.Summary = await SummaryGenerator.SummarizeAsync(method.Code, ollama);
                                mergedMethods.Add(method);
                                changeCount++;
                            }
                        }
                    }

                    newType.Methods = mergedMethods;
                    updated.Add(newType);
                    changeCount++;
                }
            }

            finalOutput.Add(new Dictionary<string, List<TypeRepresentation>> { [fileKey] = updated });
        }

        return new ChangeResult
        {
            HasChanges = changeCount > 0,
            ChangeCount = changeCount,
            MergedJson = finalOutput
        };
    }

    private static Dictionary<string, List<TypeRepresentation>> LoadExisting(string path)
    {
        if (!File.Exists(path)) return new();

        try
        {
            var raw = File.ReadAllText(path);
            return JsonSerializer.Deserialize<List<Dictionary<string, List<TypeRepresentation>>>>(raw)?
                       .SelectMany(dict => dict)
                       .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                   ?? new();
        }
        catch
        {
            Console.WriteLine("⚠ Failed to load or parse existing JSON, ignoring it.");
            return new();
        }
    }

    private static Dictionary<string, List<TypeRepresentation>> GroupByFile(IEnumerable<TypeRepresentation> items)
    {
        var grouped = new Dictionary<string, List<TypeRepresentation>>();

        foreach (var item in items)
        {
            var key =
                $"{Path.GetFileName(item.SourcePath)}@{Path.GetDirectoryName(item.SourcePath)?.Replace('\\', '/') ?? "."}";

            if (!grouped.TryGetValue(key, out var list))
            {
                list = new List<TypeRepresentation>();
                grouped[key] = list;
            }

            list.Add(item);
        }

        return grouped;
    }
}