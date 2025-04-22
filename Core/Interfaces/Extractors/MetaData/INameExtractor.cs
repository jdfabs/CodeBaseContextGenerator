namespace CodeBaseContextGenerator.Core.Interfaces.Extractors.MetaData;

public interface INameExtractor<in TNode>
{
    protected string ExtractName(TNode node);
}