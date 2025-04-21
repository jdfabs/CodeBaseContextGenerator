using Antlr4.Runtime;
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
    private readonly CommonTokenStream _tokenStream;
    private readonly List<IAstTypeNode> _nodes = new();
    

    public CompilationUnitVisitor(string rootPath, string sourcePath, CommonTokenStream tokenStream)
    {
        _rootPath   = rootPath;
        _sourcePath = sourcePath;
        _tokenStream = tokenStream;
    }

    /// <summary>All types gathered from the compilation unit.</summary>
    public IReadOnlyCollection<IAstTypeNode> CollectedNodes => _nodes;

    /*────────────────────────  Dispatch  ─────────────────────────*/

    public override object? VisitTypeDeclaration(JavaParser.TypeDeclarationContext ctx)
    {
        if (ctx.classDeclaration() is { } cls)
        {
            var visitor = new ClassVisitor(_rootPath, _sourcePath,_tokenStream);
            _nodes.Add(visitor.VisitClassDeclaration(cls));
        }
        else if (ctx.interfaceDeclaration() is { } iface)
        {
            var visitor = new InterfaceVisitor(_tokenStream);
            _nodes.Add(visitor.VisitInterfaceDeclaration(iface));
        }
        else if (ctx.enumDeclaration() is { } enums)
        {
            var visitor = new EnumVisitor(_rootPath, _sourcePath,_tokenStream);
            _nodes.Add(visitor.VisitEnumDeclaration(enums));
        }
        else if (ctx.recordDeclaration() is {} record)
        {
            var visitor = new RecordVisitor(_rootPath, _sourcePath,_tokenStream);
            _nodes.Add(visitor.VisitRecordDeclaration(record));
        }
        else
        {
            throw new NotSupportedException($"Unsupported type declaration: {ctx.GetText()}");
        }
        
        return null; // prevent default deep‑walk – visitors handle children
    }
}
