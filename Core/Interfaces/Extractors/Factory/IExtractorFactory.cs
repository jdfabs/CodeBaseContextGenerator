namespace CodeBaseContextGenerator.Core.Interfaces.Extractors;

public interface IExtractorFactory
{
    IExtractorSet<TNode> CreateFor<TNode>();
}