using CodeBaseContextGenerator.Core.Interfaces.Representations;

namespace CodeBaseContextGenerator.Core.Interfaces.Extractors.MetaData;

public interface IReturnTypeExtractor<in TNode>
{
    protected ITypeReference ExtractReturnType(TNode node);
}