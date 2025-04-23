using System.Collections.Immutable;
using CodeBaseContextGenerator.Core.Models.Representations;

namespace CodeBaseContextGenerator.JavaAntlr4.Builders;

/// <summary>
/// Converts light‑weight AST nodes produced by the parse visitors into the
/// the rest of the pipeline expects.
/// </summary>
public sealed class TypeRepresentationBuilder(string rootPath, string sourcePath)
{
    /// <summary>
    /// The resulting list contains both types and methods – preserving the
    /// same structure used by the legacy implementation.
    /// </summary>
    public IReadOnlyList<TypeRepresentationBase> BuildAll(IEnumerable<TypeRepresentationBase> nodes)
    {
        var output = new List<TypeRepresentationBase>();

        foreach (var node in nodes)
            output.Add(node);

        return output;
    }
}