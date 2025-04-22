using CodeBaseContextGenerator.Core.Interfaces.Representations;

namespace CodeBaseContextGenerator.Core.Interfaces.Extractors.Structure;

public interface IFieldExtractor<in TNode> {
    protected IReadOnlyCollection<IFieldRepresentation> ExtractorField(TNode node);
}