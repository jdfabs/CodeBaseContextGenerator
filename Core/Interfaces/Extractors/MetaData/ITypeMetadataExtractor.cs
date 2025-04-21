namespace CodeBaseContextGenerator.Core.Interfaces.Extractors.MetaData;

public interface ITypeMetadataExtractor<in TNode> : IExtractor<TNode> {
    string ExtractKind(TNode node); // class, interface, enum, record
    bool IsAbstract(TNode node);
}