using CodeBaseContextGenerator.JavaAntlr4.Builders;

namespace CodeBaseContextGenerator.JavaAntlr4.Nodes;

public class RecordNode : IAstTypeNode
{
    public string Name { get; set; }
    public string Kind { get; } = "record";
    public string Privacy { get; init; }
    public string Content { get; init; }
    public IReadOnlyCollection<string> Modifiers { get; init; }
    public bool IsAbstract { get; init; }
    public string? Javadoc { get; init; }
    public IReadOnlyCollection<AstFieldNode> Fields { get; init; }
    public IReadOnlyCollection<AstConstructorNode> Constructors { get; init; }
    public IReadOnlyCollection<TypeReference> InheritanceRefs { get; init; }
    public IReadOnlyCollection<AstMethodNode> Methods { get; init; }

}