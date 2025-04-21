using CodeBaseContextGenerator.Core.Interfaces.Representations;

namespace CodeBaseContextGenerator.Core.Interfaces.Extractors.Structure;

public interface IMethodExtractor<in TNode> : IExtractor<TNode, IReadOnlyCollection<IMethodRepresentation>> {
}
