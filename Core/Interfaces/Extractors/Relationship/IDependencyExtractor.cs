using CodeBaseContextGenerator.Core.Interfaces.Representations;

namespace CodeBaseContextGenerator.Core.Interfaces.Extractors.Relationship;

public interface IDependencyExtractor<in TNode> : IExtractor<TNode> {
    IReadOnlyCollection<ITypeReference> ExtractDependencies(TNode node); // base + usages
}
