using CodeBaseContextGenerator.JavaAntlr4.Builders;

namespace CodeBaseContextGenerator.JavaAntlr4.Visitors;

/// <summary>
/// Top‑level visitor that walks a Java parse tree and collects a flat list
/// of <see cref="IAstTypeNode"/> objects (classes or interfaces).
/// </summary>
public sealed class CompilationUnitVisitor : JavaParserBaseVisitor<object?>
{
    private readonly string _rootPath;
    private readonly string _sourcePath;
    private readonly List<IAstTypeNode> _nodes = new();

    public CompilationUnitVisitor(string rootPath, string sourcePath)
    {
        _rootPath   = rootPath;
        _sourcePath = sourcePath;
    }

    /// <summary>All types gathered from the compilation unit.</summary>
    public IReadOnlyCollection<IAstTypeNode> CollectedNodes => _nodes;

    /*────────────────────────  Dispatch  ─────────────────────────*/

    public override object? VisitTypeDeclaration(JavaParser.TypeDeclarationContext ctx)
    {
        if (ctx.classDeclaration() is { } cls)
        {
            var visitor = new ClassVisitor(_rootPath, _sourcePath);
            _nodes.Add(visitor.VisitClassDeclaration(cls));
        }
        else if (ctx.interfaceDeclaration() is { } iface)
        {
            var visitor = new InterfaceVisitor(_rootPath, _sourcePath);
            _nodes.Add(visitor.VisitInterfaceDeclaration(iface));
        }
        return null; // prevent default deep‑walk – visitors handle children
    }
}
