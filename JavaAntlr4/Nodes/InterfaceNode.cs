using Antlr4.Runtime.Misc;
using CodeBaseContextGenerator.JavaAntlr4.Builders;

namespace CodeBaseContextGenerator.JavaAntlr4.Nodes;
/// <summary>
/// Concrete node for Java <c>interface</c> declarations.
/// </summary>
public sealed record InterfaceNode : IAstTypeNode
{
    public string Name { get; init; } = string.Empty;
    public string Kind => "interface";

    public string Privacy { get; init; } = "package-private";
    public IReadOnlyCollection<string> Modifiers { get; init; } = new List<string>();
    public Interval BodyInterval { get; init; }
    public string Content { get; init; } = string.Empty;

    public IReadOnlyCollection<TypeReference> InheritanceRefs { get; init; } = new List<TypeReference>();
    public IReadOnlyCollection<AstMethodNode> Methods { get; init; } = new List<AstMethodNode>();
}