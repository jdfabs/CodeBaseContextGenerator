using CodeBaseContextGenerator.JavaAntlr4.Builders;
using CodeBaseContextGenerator.JavaAntlr4.Extrators;
using CodeBaseContextGenerator.JavaAntlr4.Nodes;

namespace CodeBaseContextGenerator.JavaAntlr4.Visitors;

internal sealed class ClassVisitor : JavaParserBaseVisitor<ClassNode>
{
    private readonly string _rootPath;
    private readonly string _sourcePath;

    public ClassVisitor(string rootPath, string sourcePath)
    {
        _rootPath   = rootPath;
        _sourcePath = sourcePath;
    }

    public override ClassNode VisitClassDeclaration(JavaParser.ClassDeclarationContext ctx)
    {
        var methods = ctx.classBody()?.classBodyDeclaration()
            .Where(b => b.memberDeclaration()?.methodDeclaration() != null)
            .Select(b => b.memberDeclaration().methodDeclaration())
            .Select(md => MethodVisitor.Build(md, _rootPath, _sourcePath))
            .ToArray() ?? System.Array.Empty<AstMethodNode>();

        return new ClassNode
        {
            Name            = ctx.identifier().GetText(),
            Privacy         = ModifierExtractor.From(ctx).FirstOrDefault(m => m is "public" or "private" or "protected") ?? "package-private",
            Modifiers       = ModifierExtractor.From(ctx),
            BodyInterval    = ctx.SourceInterval,
            Content         = ctx.GetText(),
            InheritanceRefs = InheritanceExtractor.From(ctx),
            Methods         = methods
        };
    }
}