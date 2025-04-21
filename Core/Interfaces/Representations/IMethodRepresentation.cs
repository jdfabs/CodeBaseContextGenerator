using CodeBaseContextGenerator.Core.Interfaces.Representations.Properties;

namespace CodeBaseContextGenerator.Core.Interfaces.Representations;

public interface IMethodRepresentation : ICodeElement, IHasCode, IHasReferences, ISourceAnchor
{
    ITypeReference ReturnType { get; }
    IReadOnlyCollection<Parameter> Parameters { get; }
    IReadOnlyCollection<string> ExceptionsThrown { get; }
}