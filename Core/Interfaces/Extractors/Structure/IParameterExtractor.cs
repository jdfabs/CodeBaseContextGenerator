using CodeBaseContextGenerator.Core.Interfaces.Representations;

namespace CodeBaseContextGenerator.Core.Interfaces.Extractors.Structure;

public interface IParameterExtractor<in TNode>
{
    protected IReadOnlyCollection<Parameter> ExtractParameters(TNode node);
}