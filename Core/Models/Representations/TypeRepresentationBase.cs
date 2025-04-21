using System.Collections.Immutable;
using CodeBaseContextGenerator.Core.Interfaces.Representations;
using CodeBaseContextGenerator.Core.Interfaces.Representations.Properties;

namespace CodeBaseContextGenerator.Core.Models.Representations;

public abstract class TypeRepresentationBase : ITypeRepresentation
{
    // ICodeElement
    public required string Name { get; init; }
    public required ImmutableHashSet<CodeModifier> Modifiers { get; init; }
    public string? Docs { get; init; }
    public required string Privacy { get; init; }

    // IHasCode
    public required string Code { get; init; }
    public string Hash => ComputeHash(Code);

    // IHasReferences
    public IReadOnlyCollection<ITypeReference> ReferenceTypes { get; init; } = Array.Empty<ITypeReference>();

    // ISourceAnchor
    public required string FilePath { get; init; }
    public int? StartLine { get; init; }
    public int? EndLine { get; init; }
    public int? StartColumn { get; init; }
    public int? EndColumn { get; init; }
    public int? Line { get; init; }
    public int? Column { get; init; }

    // ITypeRepresentation specific
    public required string Type { get; init; } // "class", "interface", etc.
    public string Summary { get; set; } = string.Empty;
    public string SourcePath => FilePath;

    public IReadOnlyCollection<ITypeReference> BaseTypes { get; init; } = Array.Empty<ITypeReference>();
    public IReadOnlyCollection<IFieldRepresentation> Fields { get; init; } = Array.Empty<IFieldRepresentation>();
    public IReadOnlyCollection<IMethodRepresentation> Methods { get; init; } = Array.Empty<IMethodRepresentation>();

    public IReadOnlyCollection<IConstructorRepresentation> Constructors { get; init; } =
        Array.Empty<IConstructorRepresentation>();

    public IReadOnlyCollection<ITypeRepresentation> NestedTypes { get; init; } = Array.Empty<ITypeRepresentation>();

    private static string ComputeHash(string code)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(code);
        return Convert.ToHexString(sha.ComputeHash(bytes));
    }
}