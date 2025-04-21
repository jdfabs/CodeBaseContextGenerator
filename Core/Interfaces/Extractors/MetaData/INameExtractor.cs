namespace CodeBaseContextGenerator.Core.Interfaces.Extractors.MetaData;

public interface INameExtractor<in TNode> : IExtractor<TNode> {
    string ExtractName(TNode node);
}
