using CodeBaseContextGenerator.JavaAntlr4.Builders;

namespace CodeBaseContextGenerator.JavaAntlr4.Nodes;

public sealed record InterfaceNode : IAstTypeNode
{
    public string Name { get; init; } = string.Empty;
    public string Kind => "interface";
    public string Privacy { get; init; } = "package-private";
    public IReadOnlyCollection<string> Modifiers { get; init; }
    public bool IsAbstract { get; init; }
    public string? Javadoc { get; init; }
    public IReadOnlyCollection<AstFieldNode> Fields { get; init; }
    public IReadOnlyCollection<AstConstructorNode> Constructors { get; init; }
    public string Content { get; init; } = string.Empty;
    public IReadOnlyCollection<TypeReference> InheritanceRefs { get; init; }
    public IReadOnlyCollection<AstMethodNode> Methods { get; init; }
}