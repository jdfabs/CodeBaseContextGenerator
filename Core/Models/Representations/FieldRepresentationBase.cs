using CodeBaseContextGenerator.Core.Interfaces.Representations;

namespace CodeBaseContextGenerator.Core.Models.Representations;

public abstract class FieldRepresentationBase : IFieldRepresentation
{
    public required string Name { get; init; }
    public required IReadOnlyCollection<string> Modifiers { get; init; }
    public string? Docs { get; init; }

    public required string Type { get; init; }
}