using System.Text.Json;
using CodeBaseContextGenerator.Cli.Models;
using CodeBaseContextGenerator.Core.Utils.Json;

namespace CodeBaseContextGenerator.Cli.Project;

public static class ContextExporter
{
    public static void Export(string jsonIn, ExplorerNode method, string outputPath)
    {
        var parsed = JsonLoader.Load<List<Dictionary<string, List<TypeRepresentation>>>>(jsonIn);
        if (parsed == null)
            throw new InvalidOperationException("❌ Failed to load or parse context JSON.");

        var allGroups = parsed
            .SelectMany(d => d)
            .ToDictionary(k => k.Key, v => v.Value);

        var lookup = allGroups
            .SelectMany(g => g.Value)
            .SelectMany(t =>
            {
                var items = new List<TypeRepresentation> { t };
                if (t.Methods != null) items.AddRange(t.Methods);
                return items;
            })
            .Where(tr => !string.IsNullOrWhiteSpace(tr.Name) && !string.IsNullOrWhiteSpace(tr.SourcePath))
            .ToDictionary(tr => $"{tr.Name}@{tr.SourcePath}", tr => tr);

        var methodRep = lookup.Values.FirstOrDefault(m =>
            m.Type == "method" &&
            m.Name == method.Data.Name &&
            m.SourcePath == method.Data.SourcePath);

        if (methodRep == null)
            throw new InvalidOperationException($"Method not found: {method.Data.Name}@{method.Data.SourcePath}");

        var className = methodRep.Name.Split('.')[0];
        var declaringKey = $"{className}@{methodRep.SourcePath}";
        lookup.TryGetValue(declaringKey, out var declaringRep);

        var baseTypes = (declaringRep?.ReferencedTypes ?? Enumerable.Empty<TypeReference>())
            .Where(r => r.Kind is "extends" or "implements" && !string.IsNullOrWhiteSpace(r.Source))
            .Select(r => lookup.TryGetValue(r.Source, out var rep) ? rep : null)
            .Where(r => r != null)
            .ToList();

        var usedTypes = methodRep.ReferencedTypes
            .Where(r => r.Kind is "uses" or "throws" && !string.IsNullOrWhiteSpace(r.Source))
            .Select(r => lookup.TryGetValue(r.Source, out var rep) ? rep : null)
            .Where(r => r != null)
            .ToList();
        
        var context = new
        {
            Method = methodRep,
            DeclaringType = declaringRep,
            BaseTypes = baseTypes,
            UsedTypes = usedTypes
        };

        File.WriteAllText(outputPath,
            JsonSerializer.Serialize(context, new JsonSerializerOptions { WriteIndented = true }));
    }
}