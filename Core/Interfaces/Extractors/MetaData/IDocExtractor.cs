namespace CodeBaseContextGenerator.Core.Interfaces.Extractors.MetaData;

public interface IDocExtractor<in TNode> : IExtractor<TNode> {
    string? ExtractDocumentation(TNode node); // Javadoc, XML doc, etc.
}