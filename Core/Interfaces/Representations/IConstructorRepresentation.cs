using CodeBaseContextGenerator.Core.Interfaces.Representations.Properties;

namespace CodeBaseContextGenerator.Core.Interfaces.Representations;

public interface IConstructorRepresentation : ICodeElement, IHasCode, IHasReferences, ISourceAnchor
{
    IReadOnlyCollection<Parameter> Parameters { get; }
    IReadOnlyCollection<string> ExceptionsThrown { get; }
}

public readonly record struct Parameter(ITypeReference Type, string Name);