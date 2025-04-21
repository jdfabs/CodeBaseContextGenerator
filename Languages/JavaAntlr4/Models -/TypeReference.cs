namespace CodeBaseContextGenerator.JavaAntlr4.Models;

/// <summary>
/// Lightweight link between two types (or a type and a method). It carries
/// just enough information for the post‑processing phase to resolve
/// cross‑file dependencies.
/// </summary>
/// <param name="Name">Simple identifier of the referenced type.</param>
/// <param name="Kind">Relationship kind – e.g. <c>extends</c>, <c>implements</c>, <c>uses</c>.</param>
/// <param name="Source">Optional “file anchor” in the form <c>Name@RelativePath</c>.  Can be filled later by the resolver.</param>
public sealed record TypeReference(
    string Name,
    string Kind,
    string Source = ""
)
{
    /// <summary>Create and resolve <see cref="Source"/> in one go.</summary>
    public static TypeReference Create(string name, string kind, string rootPath, string sourcePath)
    {
        var rel = Path.GetRelativePath(rootPath, sourcePath).Replace('\\', '/');
        return new TypeReference(name, kind, $"{name}@{rel}");
    }

    public override string ToString() => string.IsNullOrWhiteSpace(Source)
        ? $"{Kind}:{Name}"
        : $"{Kind}:{Source}";
}