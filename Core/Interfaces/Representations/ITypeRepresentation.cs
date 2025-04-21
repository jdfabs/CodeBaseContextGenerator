using CodeBaseContextGenerator.Core.Interfaces.Representations.Properties;

namespace CodeBaseContextGenerator.Core.Interfaces.Representations;

public interface ITypeRepresentation : ICodeElement, IHasCode, IHasReferences, ISourceAnchor
{
    string Type { get; } 

    string Summary { get; set; }

    IReadOnlyCollection<ITypeReference> BaseTypes { get; }
    IReadOnlyCollection<IFieldRepresentation> Fields { get; }
    IReadOnlyCollection<IMethodRepresentation> Methods { get; }
    IReadOnlyCollection<IConstructorRepresentation> Constructors { get; }
    IReadOnlyCollection<ITypeRepresentation> NestedTypes { get; }
}