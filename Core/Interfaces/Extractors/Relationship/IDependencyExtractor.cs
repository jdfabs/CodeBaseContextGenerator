using CodeBaseContextGenerator.Core.Interfaces.Representations;

namespace CodeBaseContextGenerator.Core.Interfaces.Extractors.Relationship;

public interface IDependencyExtractor<in TNode> : IExtractor<TNode, IReadOnlyCollection<ITypeRepresentation>> {
}
