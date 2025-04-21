using CodeBaseContextGenerator.Core.Interfaces.Representations;

namespace CodeBaseContextGenerator.Core.Interfaces.Extractors.MetaData;

public interface IReturnTypeExtractor<in TNode> : IExtractor<TNode> {
    ITypeReference ExtractReturnType(TNode node);
}