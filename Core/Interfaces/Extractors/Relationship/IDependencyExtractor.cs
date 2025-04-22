using CodeBaseContextGenerator.Core.Interfaces.Representations;

namespace CodeBaseContextGenerator.Core.Interfaces.Extractors.Relationship;

public interface IDependencyExtractor<in TNode>
{
    protected IReadOnlyCollection<ITypeRepresentation> ExtractDependencies(TNode node);
}