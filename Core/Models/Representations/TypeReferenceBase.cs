using System.Text.Json.Serialization;
using CodeBaseContextGenerator.Core.Interfaces.Representations;

namespace CodeBaseContextGenerator.Core.Models.Representations;

public abstract class TypeReferenceBase : ITypeReference
{
    public required string Name { get; init; }
    public required string Type { get; init; }
    public required ReferenceKind Kind { get; init; }
    public required string Source { get; init; }

    public string FullyQualifiedName => $"{Type}.{Name}";
    public string? TargetPath { get; init; }
    public string? TargetLanguage { get; init; }

    [JsonIgnore]
    public ITypeRepresentation? ResolvedTarget { get; init; }

    public override string ToString() => string.IsNullOrWhiteSpace(Source)
        ? $"{Kind}:{Name}"
        : $"{Kind}:{Source}";
}