using CodeBaseContextGenerator.Core.Interfaces.Representations;

namespace CodeBaseContextGenerator.Core.Interfaces.Extractors;

public interface IContextExtractor<in TRoot> : IExtractor<TRoot,IReadOnlyCollection<ITypeRepresentation>>{
}
