namespace CodeBaseContextGenerator.JavaAntlr4.Base;

public static class RefResolver
{
    public static void Resolve(List<TypeRepresentation> allItems)
    {
        var allTypesByName = allItems
            .Where(t => t.Type != "method")
            .GroupBy(t => t.Name)
            .ToDictionary(g => g.Key, g => g.First());

        var resolved = new HashSet<string>();
        var unresolved = new HashSet<string>();

        foreach (var type in allItems)
        {
            ResolveRefs(type.ReferencedTypes, type.Name);
            if (type.Methods == null) continue;

            foreach (var method in type.Methods)
                ResolveRefs(method.ReferencedTypes, method.Name);
        }

        // ───── Print results ─────
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.WriteLine("\n=== 🔎 Reference Resolution ===");

        foreach (var name in resolved)
            Console.WriteLine($"✓ Resolved: {name}");

        Console.ForegroundColor = ConsoleColor.Yellow;
        foreach (var name in unresolved)
            Console.WriteLine($"⚠ Unresolved: {name}");

        if (!resolved.Any() && !unresolved.Any())
            Console.WriteLine("No references to resolve.");

        Console.ResetColor();

        // ───── Inner logic ─────
        void ResolveRefs(IEnumerable<TypeReference>? refs, string owner)
        {
            if (refs == null) return;

            foreach (var r in refs)
            {
                if (string.IsNullOrWhiteSpace(r.Name)) continue;

                var name = r.Name;

                if (allTypesByName.TryGetValue(name, out var target))
                {
                    r.Source = $"{name}@{target.SourcePath}";
                    resolved.Add($"{name} → {r.Source}");
                }
                else
                {
                    unresolved.Add($"{name} (in {owner})");
                }
            }
        }
    }
}