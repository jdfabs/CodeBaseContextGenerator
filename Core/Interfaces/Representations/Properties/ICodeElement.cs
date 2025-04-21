namespace CodeBaseContextGenerator.Core.Interfaces.Representations.Properties;

public interface ICodeElement : IName
{
    IReadOnlyCollection<string> Modifiers { get; }
    string? Docs { get; }
}