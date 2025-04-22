using CodeBaseContextGenerator.Core.Interfaces.Representations;

namespace CodeBaseContextGenerator.Core.Interfaces.Extractors;

public interface IContextExtractor<in TRoot>
{
    protected IReadOnlyCollection<ITypeRepresentation> ExtractContext(TRoot node);
}
