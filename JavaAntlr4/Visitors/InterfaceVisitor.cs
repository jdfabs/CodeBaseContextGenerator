using CodeBaseContextGenerator.JavaAntlr4.Builders;
using CodeBaseContextGenerator.JavaAntlr4.Extrators;
using CodeBaseContextGenerator.JavaAntlr4.Nodes;

namespace CodeBaseContextGenerator.JavaAntlr4.Visitors;

internal sealed class InterfaceVisitor : JavaParserBaseVisitor<InterfaceNode>
{
    private readonly string _rootPath;
    private readonly string _sourcePath;

    public InterfaceVisitor(string rootPath, string sourcePath)
    {
        _rootPath   = rootPath;
        _sourcePath = sourcePath;
    }

    public override InterfaceNode VisitInterfaceDeclaration(JavaParser.InterfaceDeclarationContext ctx)
    {
        // ❗ Interface rules in the Java grammar use <interfaceMethodDeclaration>
        //    (or its generic variant) rather than the <methodDeclaration>
        //    rule that classes use.  To keep this visitor simple – and avoid
        //    a large amount of mapping code – we currently *omit* interface
        //    methods from the output.  They can be added later by writing a
        //    dedicated InterfaceMethodVisitor.

        return new InterfaceNode
        {
            Name            = ctx.identifier().GetText(),
            Privacy         = ModifierExtractor.From(ctx).FirstOrDefault(m => m is "public" or "private" or "protected") ?? "package-private",
            Modifiers       = ModifierExtractor.From(ctx),
            BodyInterval    = ctx.SourceInterval,
            Content         = ctx.GetText(),
            InheritanceRefs = InheritanceExtractor.From(ctx),
            Methods         = System.Array.Empty<AstMethodNode>()
        };
    }
}