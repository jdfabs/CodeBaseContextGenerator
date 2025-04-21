namespace CodeBaseContextGenerator.Core.Interfaces.Representations.Properties;

public interface IHasName
{
    string Name { get; } 
    string FullQualifiedName { get; }
}