using CodeBaseContextGenerator.Core.Interfaces.Representations;

namespace CodeBaseContextGenerator.Core.Interfaces.Extractors;

public interface IContextExtractor<in TRoot> {
    IReadOnlyCollection<ITypeRepresentation> ExtractTypes(TRoot rootNode);
}
