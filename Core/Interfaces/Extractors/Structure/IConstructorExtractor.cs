using CodeBaseContextGenerator.Core.Interfaces.Representations;

namespace CodeBaseContextGenerator.Core.Interfaces.Extractors.Structure;

public interface IConstructorExtractor<in TNode> : IExtractor<TNode> {
    IReadOnlyCollection<IConstructorRepresentation> ExtractConstructors(TNode node);
}