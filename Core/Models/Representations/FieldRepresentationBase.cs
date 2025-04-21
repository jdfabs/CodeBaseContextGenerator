using System.Collections.Immutable;
using CodeBaseContextGenerator.Core.Interfaces.Representations;
using CodeBaseContextGenerator.Core.Interfaces.Representations.Properties;

namespace CodeBaseContextGenerator.Core.Models.Representations;

public abstract class FieldRepresentationBase : IFieldRepresentation
{
    public required string Name { get; init; }
    public required ImmutableHashSet<CodeModifier> Modifiers { get; init; }
    public string? Docs { get; init; }

    public ITypeReference Type { get; }
}