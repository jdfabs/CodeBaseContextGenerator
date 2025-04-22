namespace CodeBaseContextGenerator.Core.Interfaces.Extractors.Structure;

public interface ICodeExtractor<in TNode>
{
    protected string ExtractCode(TNode node);
}