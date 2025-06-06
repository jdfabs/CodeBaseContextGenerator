using CodeBaseContextGenerator.Core.Utils.Json;
using CodeBaseContextGenerator.LLM;

namespace CodeBaseContextGenerator.Core.Shared;

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
        var merged = new Dictionary<string, List<TypeRepresentation>>(); // 💡 instead of finalOutput

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


            merged[fileKey] = updated;
        }

        if (changeCount == 0) return ChangeResult.NoChanges;

        var finalOutput = merged
            .Select(kvp => new Dictionary<string, List<TypeRepresentation>> { [kvp.Key] = kvp.Value })
            .ToList();

        return new ChangeResult
        {
            HasChanges = true,
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

    private static async Task<TypeRepresentation> SummarizeIfChangedAsync(TypeRepresentation? old, TypeRepresentation current, OllamaClient ollama)
    {
        if (old?.Hash == current.Hash && MethodsUnchanged(old, current))
            return old;

        current.Summary = await SummaryGenerator.SummarizeAsync(current.Code, ollama);
        current.Methods = await SummarizeChangedMethodsAsync(old?.Methods, current.Methods, ollama);
        return current;
    }

    private static bool MethodsUnchanged(TypeRepresentation? old, TypeRepresentation current)
    {
        if (old?.Methods == null || current.Methods == null) return false;

        var currentMap = current.Methods.ToDictionary(m => m.Name);
        return old.Methods.All(oldM =>
            currentMap.TryGetValue(oldM.Name, out var currM) &&
            oldM.Hash == currM.Hash
        );
    }
    
    private static async Task<List<TypeRepresentation>> SummarizeChangedMethodsAsync(
        List<TypeRepresentation>? oldMethods,
        List<TypeRepresentation>? currentMethods,
        OllamaClient ollama)
    {
        if (currentMethods == null) return [];

        var tasks = currentMethods.Select(async method =>
        {
            var oldMethod = oldMethods?.FirstOrDefault(m => m.Name == method.Name && m.Type == "method");
            if (oldMethod?.Hash == method.Hash) return oldMethod;
            Console.WriteLine($"Summarizing method {method.Name}...");
            method.Summary = await SummaryGenerator.SummarizeAsync(method.Code, ollama);
            return method;
        });

        return (await Task.WhenAll(tasks)).ToList();
    }
}