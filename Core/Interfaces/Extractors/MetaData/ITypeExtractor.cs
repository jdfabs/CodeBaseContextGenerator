namespace CodeBaseContextGenerator.Core.Interfaces.Extractors.MetaData;

public interface ITypeExtractor<in TNode>
{
    protected string ExtractType(TNode node);
}