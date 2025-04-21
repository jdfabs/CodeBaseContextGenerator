namespace CodeBaseContextGenerator.Core.Interfaces.Extractors.MetaData;

public interface IPrivacyExtractor<in TNode> : IExtractor<TNode> {
    string ExtractPrivacy(TNode node); // "public", "private", etc.
}
