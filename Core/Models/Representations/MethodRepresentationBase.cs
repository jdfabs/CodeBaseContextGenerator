using System.Collections.Immutable;
using CodeBaseContextGenerator.Core.Interfaces.Representations;
using CodeBaseContextGenerator.Core.Interfaces.Representations.Properties;

namespace CodeBaseContextGenerator.Core.Models.Representations;

public abstract class MethodRepresentationBase : IMethodRepresentation
{
    // ICodeElement
    public required string Name { get; init; }
    public required ImmutableHashSet<CodeModifier> Modifiers { get; init; }
    public string? Docs { get; init; }

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

    // IMethodRepresentation specific
    public required ITypeReference ReturnType { get; init; }
    public required IReadOnlyCollection<Parameter> Parameters { get; init; }
    public IReadOnlyCollection<string> ExceptionsThrown { get; init; } = Array.Empty<string>();

    private static string ComputeHash(string code)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(code);
        return Convert.ToHexString(sha.ComputeHash(bytes));
    }
}