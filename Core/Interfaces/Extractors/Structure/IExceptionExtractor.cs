namespace CodeBaseContextGenerator.Core.Interfaces.Extractors.Structure;

public interface IExceptionExtractor<in TNode> : IExtractor<TNode> {
    IReadOnlyCollection<string> ExtractThrownExceptions(TNode node);
}