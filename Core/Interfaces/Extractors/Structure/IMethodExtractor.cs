using CodeBaseContextGenerator.Core.Interfaces.Representations;

namespace CodeBaseContextGenerator.Core.Interfaces.Extractors.Structure;

public interface IMethodExtractor<in TNode>
{
    protected IReadOnlyCollection<IMethodRepresentation> ExtractMethod(TNode node);
}