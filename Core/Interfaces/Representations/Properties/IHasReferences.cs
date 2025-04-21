namespace CodeBaseContextGenerator.Core.Interfaces.Representations.Properties;

public interface IHasReferences {
    IReadOnlyCollection<ITypeReference> ReferenceTypes { get; }
}
