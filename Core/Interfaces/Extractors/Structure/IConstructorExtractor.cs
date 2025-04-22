using CodeBaseContextGenerator.Core.Interfaces.Representations;

namespace CodeBaseContextGenerator.Core.Interfaces.Extractors.Structure;

public interface IConstructorExtractor<in TNode> {
    protected IReadOnlyCollection<IConstructorRepresentation> ExtractConstructor(TNode node);
}