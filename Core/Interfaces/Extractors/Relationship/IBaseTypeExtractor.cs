using CodeBaseContextGenerator.Core.Interfaces.Representations;

namespace CodeBaseContextGenerator.Core.Interfaces.Extractors.Relationship;

public interface IBaseTypeExtractor<in TNode>
{
    protected IReadOnlyCollection<ITypeReference> ExtractInheritedTypes(TNode node);
}