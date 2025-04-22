namespace CodeBaseContextGenerator.Core.Interfaces.Extractors.MetaData;

public interface IDocExtractor<in TNode> {
    protected string ExtractDoc(TNode node);
}