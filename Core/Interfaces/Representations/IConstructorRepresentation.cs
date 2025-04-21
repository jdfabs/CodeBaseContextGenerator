using CodeBaseContextGenerator.Core.Interfaces.Representations.Properties;

namespace CodeBaseContextGenerator.Core.Interfaces.Representations;

public interface IConstructorRepresentation : ICodeElement, IHasCode, IHasReferences, ISourceAnchor
{
    string Parameters { get; }
    IReadOnlyCollection<string> ExceptionsThrown { get; }
}