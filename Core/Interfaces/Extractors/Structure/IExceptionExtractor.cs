using CodeBaseContextGenerator.Core.Interfaces.Representations;

namespace CodeBaseContextGenerator.Core.Interfaces.Extractors.Structure;

public interface IExceptionExtractor<in TNode> {
    protected IReadOnlyCollection<string> ExtractExceptions(TNode node);
}