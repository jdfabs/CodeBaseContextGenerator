using CodeBaseContextGenerator.Core.Interfaces.Representations;

namespace CodeBaseContextGenerator.Core.Interfaces.Extractors.Structure;

public interface IFieldExtractor<in TNode> : IExtractor<TNode> {
    IReadOnlyCollection<IFieldRepresentation> ExtractFields(TNode node);
}