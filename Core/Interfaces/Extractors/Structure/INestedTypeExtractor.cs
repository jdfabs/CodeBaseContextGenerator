using CodeBaseContextGenerator.Core.Interfaces.Representations;

namespace CodeBaseContextGenerator.Core.Interfaces.Extractors.Structure;

public interface INestedTypeExtractor<in TNode> : IExtractor<TNode, IReadOnlyCollection<ITypeRepresentation>> {
}