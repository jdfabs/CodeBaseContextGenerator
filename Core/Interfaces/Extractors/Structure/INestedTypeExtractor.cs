using CodeBaseContextGenerator.Core.Interfaces.Representations;

namespace CodeBaseContextGenerator.Core.Interfaces.Extractors.Structure;

public interface INestedTypeExtractor<in TNode>
{
    protected IReadOnlyCollection<ITypeRepresentation> ExtractNestedType(TNode node);
}