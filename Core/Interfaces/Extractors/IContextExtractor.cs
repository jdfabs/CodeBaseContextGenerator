using CodeBaseContextGenerator.Core.Interfaces.Extractors.MetaData;
using CodeBaseContextGenerator.Core.Interfaces.Extractors.Relationship;
using CodeBaseContextGenerator.Core.Interfaces.Extractors.Structure;
using CodeBaseContextGenerator.Core.Interfaces.Representations;

namespace CodeBaseContextGenerator.Core.Interfaces.Extractors;

public interface IContextExtractor<in TRoot>:
    IDocExtractor<TRoot>, 
    IFullQualifiedNameExtractor<TRoot>,
    IModifierExtractor<TRoot>,
    INameExtractor<TRoot>,
    IPrivacyExtractor<TRoot>,
    IReturnTypeExtractor<TRoot>,
    ITypeExtractor<TRoot>,
    IBaseTypeExtractor<TRoot>,
    IDependencyExtractor<TRoot>,
    IImportExtractor<TRoot>,
    ISourceAnchorExtractor<TRoot>,
    ITypeUsageExtractor<TRoot>,
    IAttributeExtractor<TRoot>,
    ICodeExtractor<TRoot>,
    IConstructorExtractor<TRoot>,
    IExceptionExtractor<TRoot>,
    IFieldExtractor<TRoot>,
    IMethodExtractor<TRoot>,
    INestedTypeExtractor<TRoot>,
    IParameterExtractor<TRoot>
{
}
