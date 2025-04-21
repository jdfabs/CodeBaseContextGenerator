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
            Modifiers      = node.Modifiers.ToList(),
            IsAbstract     = node.IsAbstract,
            Javadoc        = node.Javadoc,
            SourcePath = Path.GetRelativePath(rootPath, sourcePath),
            Code = node.Content,
            ReferencedTypes = node.InheritanceRefs.ToList(),
            Fields         = node.Fields.Select(f => new FieldRepresentation {
                Name       = f.Name,
                Type       = f.Type,
                Privacy    = f.Privacy,
                Modifiers  = f.Modifiers.ToList(),
                Javadoc    = f.Javadoc
            }).ToList(),
            Constructors   = node.Constructors.Select(c => new ConstructorRepresentation {
                Name             = c.Name,
                Privacy          = c.Privacy,
                Modifiers        = c.Modifiers.ToList(),
                Parameters       = c.Parameters,
                ExceptionsThrown = string.IsNullOrEmpty(c.Throws)
                    ? new List<string>()
                    : c.Throws.Split(',').Select(x=>x.Trim()).ToList(),
                Javadoc          = c.Javadoc
            }).ToList(),
            Methods        = new List<TypeRepresentation>()
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
    IReadOnlyCollection<string> Modifiers { get; }               // ← NEW
    bool IsAbstract { get; }                                      // ← NEW
    string? Javadoc { get; }                                      // ← NEW
    IReadOnlyCollection<AstFieldNode> Fields { get; }             // ← NEW
    IReadOnlyCollection<AstConstructorNode> Constructors { get; } // ← NEW
    IReadOnlyCollection<TypeReference> InheritanceRefs { get; }
    IReadOnlyCollection<AstMethodNode> Methods { get; }
}

public sealed record AstFieldNode(
    string Name,
    string Type,
    string Privacy,
    IReadOnlyCollection<string> Modifiers,
    string? Javadoc,
    string Content
);

public sealed record AstConstructorNode(
    string Name,
    string Privacy,
    IReadOnlyCollection<string> Modifiers,
    string Parameters,
    string? Throws,      // e.g. "Exception"
    string? Javadoc,
    string Content
);

/// <summary>Light‑weight method node.</summary>
public sealed record AstMethodNode(
    string Name,
    string ReturnType,
    string Parameters,
    string Throws,      // e.g. "Exception"
    string Javadoc,
    IReadOnlyCollection<string> Modifiers,
    string Content,
    IReadOnlyCollection<TypeReference> ReferencedTypes
);