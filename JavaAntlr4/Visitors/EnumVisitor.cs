using Antlr4.Runtime;
using CodeBaseContextGenerator.JavaAntlr4.Builders;
using CodeBaseContextGenerator.JavaAntlr4.Extrators;
using CodeBaseContextGenerator.JavaAntlr4.Nodes;

namespace CodeBaseContextGenerator.JavaAntlr4.Visitors;

internal sealed class EnumVisitor(string rootPath, string sourcePath, CommonTokenStream tokenStream) : JavaParserBaseVisitor<EnumNode>
{
    public override EnumNode VisitEnumDeclaration(JavaParser.EnumDeclarationContext ctx)
    {
        string? javadoc = JavadocExtractor.From(ctx, tokenStream);

        // 2) Enum constants → treat each constant as a “field”

        // 3) Body declarations (methods/ctors) if present
        var bodyDecls = ctx.enumBodyDeclarations()?.classBodyDeclaration()
                        ?? Enumerable.Empty<JavaParser.ClassBodyDeclarationContext>();

        // 4) Constructors
        var ctors = bodyDecls
            .Where(d => d.memberDeclaration()?.constructorDeclaration() != null)
            .Select(d => ConstructorVisitor.Build(
                d.memberDeclaration().constructorDeclaration(), tokenStream))
            .ToArray();

        // 5) Methods
        var methods = bodyDecls
            .Where(d => d.memberDeclaration()?.methodDeclaration() != null)
            .Select(d => MethodVisitor.Build(
                d.memberDeclaration().methodDeclaration(),
                rootPath,
                sourcePath, tokenStream))
            .ToArray();

        return new EnumNode
        {
            Name = ctx.identifier().GetText(),
            Content = ctx.GetText(),
            Javadoc = javadoc,
            Constructors = ctors,
            Methods = methods,
        };
    }
}