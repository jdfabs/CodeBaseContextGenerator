namespace CodeBaseContextGenerator.Core.Interfaces.Extractors.Structure;

public interface IAttributeExtractor<in TNode> : IExtractor<TNode> {
    IReadOnlyCollection<string> ExtractAttributes(TNode node); // Annotations / Attributes
}