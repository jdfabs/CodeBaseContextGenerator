using CodeBaseContextGenerator.Json;
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
                var result = await SummarizeIfChangedAsync(oldType, newType, ollama);

                if (!ReferenceEquals(result, oldType))
                    changeCount++;

                updated.Add(result);
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
        var parsed = JsonLoader.Load<List<Dictionary<string, List<TypeRepresentation>>>>(path);
        return parsed?
                   .SelectMany(dict => dict)
                   .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
               ?? new();
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

    private static async Task<TypeRepresentation> SummarizeIfChangedAsync(
        TypeRepresentation? old, TypeRepresentation current, OllamaClient ollama)
    {
        if (old?.Hash == current.Hash)
            return old;

        current.Summary = await SummaryGenerator.SummarizeAsync(current.Code, ollama);

        if (current.Methods != null)
        {
            var summaries = current.Methods.Select(async method =>
            {
                Console.WriteLine($"Summarizing method {method.Name}...");
                var oldMethod = old?.Methods?.FirstOrDefault(m => m.Name == method.Name && m.Type == "method");

                if (oldMethod?.Hash == method.Hash)
                    return oldMethod;

                method.Summary = await SummaryGenerator.SummarizeAsync(method.Code, ollama);
                return method;
            });

            current.Methods = (await Task.WhenAll(summaries)).ToList();
        }

        return current;
    }
}