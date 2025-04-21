using System.Collections.Immutable;

namespace CodeBaseContextGenerator.Core.Interfaces.Representations.Properties;

public interface ICodeElement : IHasName
{
    ImmutableHashSet<CodeModifier> Modifiers { get; }
    string? Docs { get; }
}

public enum CodeModifier
{
    Public,
    Private,
    Protected,
    Internal,
    Static,
    Abstract,
    Virtual,
    Override,
    Sealed,
    ReadOnly,
    Const
}