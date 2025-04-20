namespace CodeBaseContextGenerator.JavaAntlr4.Builders;

/// <summary>
/// Converts light‑weight AST nodes produced by the parse visitors into the
/// rich, JSON‑serialisable <see cref="TypeRepresentation"/> objects that
/// the rest of the pipeline expects.
/// </summary>
public sealed class TypeRepresentationBuilder(string rootPath, string sourcePath)
{
    /// <summary>
    /// Entry‑point: turn a flat collection of <see cref="IAstTypeNode"/>
    /// trees into a flat list of <see cref="TypeRepresentation"/> records.
    /// The resulting list contains both types and methods – preserving the
    /// same structure used by the legacy implementation.
    /// </summary>
    public IReadOnlyList<TypeRepresentation> BuildAll(IEnumerable<IAstTypeNode> nodes)
    {
        var output = new List<TypeRepresentation>();

        foreach (var node in nodes)
            output.Add(Build(node));

        return output;
    }

    /*───────────────────────────  Helpers  ───────────────────────────*/

    private TypeRepresentation Build(IAstTypeNode node)
    {
        var rep = new TypeRepresentation
        {
            Name = node.Name,
            Type = node.Kind,
            Privacy = node.Privacy,
            SourcePath = Path.GetRelativePath(rootPath, sourcePath),
            Code = node.Content,
            ReferencedTypes = node.InheritanceRefs.ToList(),
            Methods = new List<TypeRepresentation>()
        };

        // &#x2192; recurse for each method
        foreach (var m in node.Methods)
            rep.Methods!.Add(Build(m, node.Name));

        return rep;
    }

    private TypeRepresentation Build(AstMethodNode m, string ownerName)
    {
        return new TypeRepresentation
        {
            Name = $"{ownerName}.{m.Name}",
            Type = "method",
            Privacy = m.Privacy,
            ReturnType = m.ReturnType,
            Parameters = m.Parameters,
            Code = m.Content,
            SourcePath = Path.GetRelativePath(rootPath, sourcePath),
            ReferencedTypes = m.ReferencedTypes.ToList()
        };
    }
}

/*───────────────────────  AST POCOs  ───────────────────────*/

/// <summary>Base contract so the builder doesn’t care about concrete node types.</summary>
public interface IAstTypeNode
{
    string Name { get; }
    string Kind { get; }
    string Privacy { get; }
    string Content { get; }
    IReadOnlyCollection<TypeReference> InheritanceRefs { get; }
    IReadOnlyCollection<AstMethodNode> Methods { get; }
}

/// <summary>‘class’ or ‘interface’ node produced by visitors.</summary>
public sealed record AstTypeNode(
    string Name,
    string Kind,
    string Privacy,
    string Content,
    IReadOnlyCollection<TypeReference> InheritanceRefs,
    IReadOnlyCollection<AstMethodNode> Methods
) : IAstTypeNode;

/// <summary>Light‑weight method node.</summary>
public sealed record AstMethodNode(
    string Name,
    string Privacy,
    string ReturnType,
    string Parameters,
    string Content,
    IReadOnlyCollection<TypeReference> ReferencedTypes
);