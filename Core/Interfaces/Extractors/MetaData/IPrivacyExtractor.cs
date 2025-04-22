namespace CodeBaseContextGenerator.Core.Interfaces.Extractors.MetaData;

public interface IPrivacyExtractor<in TNode>
{
    protected string ExtractPrivacy(TNode node);
}