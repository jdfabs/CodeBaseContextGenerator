using CodeBaseContextGenerator.Core.Interfaces.Assemblers;
using CodeBaseContextGenerator.Core.Interfaces.Extractors.MetaData;
using CodeBaseContextGenerator.Core.Interfaces.Extractors.Relationship;
using CodeBaseContextGenerator.Core.Interfaces.Extractors.Structure;

namespace CodeBaseContextGenerator.Core.Interfaces.Extractors;

public interface IExtractorSet<TNode>
{
    INameExtractor<TNode> Name { get; }
    IModifierExtractor<TNode> Modifiers { get; }
    IDocExtractor<TNode> Docs { get; }
    ISourceAnchorExtractor<TNode> Anchor { get; }
    ITypeMetadataExtractor<TNode> Metadata { get; }
    IBaseTypeExtractor<TNode> BaseTypes { get; }
    ITypeUsageExtractor<TNode> Usages { get; }
    IFieldExtractor<TNode> Fields { get; }
    IConstructorExtractor<TNode> Constructors { get; }
    IMethodExtractor<TNode> Methods { get; }
    INestedTypeAssembler<TNode> NestedTypes { get; }
    IFullQualifiedNameExtractor<TNode> FullQualifiedName { get; }
    ICodeExtractor<TNode> Code { get; }
}