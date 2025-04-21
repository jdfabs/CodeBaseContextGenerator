using System.Text.Json.Serialization;
using CodeBaseContextGenerator.Core.Interfaces.Representations.Properties;

namespace CodeBaseContextGenerator.Core.Interfaces.Representations;

public interface ITypeReference : IHasName
{
    string Type { get; }
    ReferenceKind Kind { get; }
    string Source { get; }

    string? TargetPath { get; }
    string? TargetLanguage { get; }

    [JsonIgnore] ITypeRepresentation? ResolvedTarget { get; }
}

public enum ReferenceKind
{
    Extends,
    Implements,
    Uses,
    Calls,
    Invokes,
    References,
    Contains,
    Declares,
    IsDeclaredBy,
    IsInstantiatedBy,
    IsInstantiatedFrom,
    IsInstantiatedIn,
    IsInstantiatedTo,
    IsInstantiatedWith,
}