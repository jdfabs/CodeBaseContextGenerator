using CodeBaseContextGenerator.Core.Interfaces.Representations;

namespace CodeBaseContextGenerator.Core.Interfaces.Extractors.Structure;

public interface IParameterExtractor<in TNode> : IExtractor<TNode> {
    IReadOnlyCollection<Parameter> ExtractParameters(TNode node); // could evolve to structured parameters
}
