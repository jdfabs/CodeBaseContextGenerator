namespace CodeBaseContextGenerator.Core.Interfaces.Extractors.MetaData;

public interface IFullQualifiedNameExtractor<in TNode>
{
    protected string ExtractFullQualifiedName(TNode node);
}