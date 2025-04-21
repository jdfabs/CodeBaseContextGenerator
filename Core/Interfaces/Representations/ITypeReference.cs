using System.Text.Json.Serialization;
using CodeBaseContextGenerator.Core.Interfaces.Representations.Properties;

namespace CodeBaseContextGenerator.Core.Interfaces.Representations;

public interface ITypeReference : IName
{
    string Type { get; }
    string Kind { get; } // "extends", "uses", etc.
    string Source { get; }

    string FullyQualifiedName { get; }
    string? TargetPath { get; }
    string? TargetLanguage { get; }

    [JsonIgnore] ITypeRepresentation? ResolvedTarget { get; }
}