using CodeBaseContextGenerator.Core.Interfaces.Representations;

namespace CodeBaseContextGenerator.Core.Interfaces.Extractors.Relationship;

public interface ITypeUsageExtractor<in TNode>
{
    protected IReadOnlyCollection<ITypeReference> ExtractTypeUsages(TNode node);
}