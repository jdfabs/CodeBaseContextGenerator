using CodeBaseContextGenerator.Core.Interfaces.Representations.Properties;

namespace CodeBaseContextGenerator.Core.Interfaces.Representations;

public interface IMethodRepresentation : ICodeElement, IHasCode, IHasReferences, ISourceAnchor
{
    string ReturnType { get; }
    string Parameters { get; }
    IReadOnlyCollection<string> ExceptionsThrown { get; }
}