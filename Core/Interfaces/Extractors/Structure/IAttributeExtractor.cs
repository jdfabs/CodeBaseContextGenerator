namespace CodeBaseContextGenerator.Core.Interfaces.Extractors.Structure;

public interface IAttributeExtractor<in TNode>
{
    protected IReadOnlyCollection<string> ExtractAttribute(TNode node);
}