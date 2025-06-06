using CodeBaseContextGenerator.Core.Interfaces.Representations.Properties;

namespace CodeBaseContextGenerator.Core.Interfaces.Representations;

public interface IFieldRepresentation : ICodeElement
{
    ITypeReference Type { get; }
}