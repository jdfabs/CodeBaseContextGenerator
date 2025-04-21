using Antlr4.Runtime;
using CodeBaseContextGenerator.JavaAntlr4.Builders;
using CodeBaseContextGenerator.JavaAntlr4.Extrators;
using CodeBaseContextGenerator.JavaAntlr4.Nodes;

namespace CodeBaseContextGenerator.JavaAntlr4.Visitors;

internal sealed class RecordVisitor(string rootPath, string sourcePath, CommonTokenStream tokenStream)
    : JavaParserBaseVisitor<RecordNode>
{
    public override RecordNode VisitRecordDeclaration(JavaParser.RecordDeclarationContext ctx)
    {
        var methods = ctx.recordBody()?.classBodyDeclaration()
            .Where(b => b.memberDeclaration()?.methodDeclaration() != null)
            .Select(b => b.memberDeclaration().methodDeclaration())
            .Select(md => MethodVisitor.Build(md, rootPath, sourcePath, tokenStream))
            .ToArray() ?? System.Array.Empty<AstMethodNode>();


// 2) Javadoc (via a helper that looks for ctx.parent().getChild(0) if it's a comment)
        string? javadoc = JavadocExtractor.From(ctx, tokenStream);

// 3) Fields
        var fields = ctx.recordBody()
            .classBodyDeclaration()
            .Where(d => d.memberDeclaration()?.fieldDeclaration() != null)
            .Select(d => FieldVisitor.Build(d.memberDeclaration().fieldDeclaration(), tokenStream))
            .ToArray();

// 4) Constructors
        var ctors = ctx.recordBody()
            .classBodyDeclaration()
            .Where(d => d.memberDeclaration()?.constructorDeclaration() != null)
            .Select(d => ConstructorVisitor.Build(d.memberDeclaration().constructorDeclaration(), tokenStream))
            .ToArray();

        return new RecordNode
        {
            Name = ctx.identifier().GetText(),
            Content = ctx.GetText(),
            Javadoc = javadoc,
            Fields = fields,
            Constructors = ctors,
        };
    }
}