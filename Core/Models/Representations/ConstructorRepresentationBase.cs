using CodeBaseContextGenerator.Core.Interfaces.Representations;

namespace CodeBaseContextGenerator.Core.Models.Representations;

public abstract class ConstructorRepresentationBase : IConstructorRepresentation
{
    // ICodeElement
    public required string Name { get; init; }
    public required IReadOnlyCollection<string> Modifiers { get; init; }
    public string? Docs { get; init; }

    // IHasCode
    public required string Code { get; init; }
    public string Hash => ComputeHash(Code);

    // IHasReferences
    public IReadOnlyCollection<ITypeReference> ReferenceTypes { get; init; } = Array.Empty<ITypeReference>();

    // ISourceAnchor
    public required string FilePath { get; init; }
    public int? Line { get; init; }
    public int? Column { get; init; }

    // IConstructorRepresentation specific
    public required string Parameters { get; init; }
    public IReadOnlyCollection<string> ExceptionsThrown { get; init; } = Array.Empty<string>();

    private static string ComputeHash(string code)
    {
        using var sha = System.Security.Cryptography.SHA256.Create();
        var bytes = System.Text.Encoding.UTF8.GetBytes(code);
        return Convert.ToHexString(sha.ComputeHash(bytes));
    }
}