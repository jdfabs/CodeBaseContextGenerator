namespace CodeBaseContextGenerator.Core.Interfaces.Extractors;

public interface IExtractor<in TNode, out TResult>
{
    TResult Extract(TNode node); 
}