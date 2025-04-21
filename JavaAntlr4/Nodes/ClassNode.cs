using Antlr4.Runtime.Misc;
using CodeBaseContextGenerator.JavaAntlr4.Builders;

namespace CodeBaseContextGenerator.JavaAntlr4.Nodes;

/// <summary>
/// Concrete node for Java <c>class</c> declarations. Implements
/// <see cref="IAstTypeNode"/> so it can be consumed by
/// <see cref="TypeRepresentationBuilder"/> without additional casting.
/// </summary>
public sealed record ClassNode : IAstTypeNode
{
    public string Name { get; init; } = string.Empty;
    public string Kind => "class";
    public string Privacy { get; init; } 
    public IReadOnlyCollection<string> Modifiers { get; init; } 
    public bool IsAbstract { get; init; }
    public string? Javadoc { get; init; }
    public IReadOnlyCollection<AstFieldNode> Fields { get; init; }
    public IReadOnlyCollection<AstConstructorNode> Constructors { get; init; }
    public string Content { get; init; } = string.Empty;
    public IReadOnlyCollection<TypeReference> InheritanceRefs { get; init; }
    public IReadOnlyCollection<AstMethodNode> Methods { get; init; }

}