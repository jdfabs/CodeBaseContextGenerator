using Antlr4.Runtime;
using CodeBaseContextGenerator.JavaAntlr4.Builders;
using CodeBaseContextGenerator.JavaAntlr4.Extrators;

namespace CodeBaseContextGenerator.JavaAntlr4.Visitors;

public static class FieldVisitor
{
    public static AstFieldNode Build(
        JavaParser.FieldDeclarationContext ctx,
        CommonTokenStream tokens)
    {
        // modifiers live on the parent ClassBodyDeclaration
        var declCtx   = ctx.Parent.Parent as JavaParser.ClassBodyDeclarationContext;
        var mods      = ModifierExtractor.From(declCtx);
        var privacy   = mods.FirstOrDefault(m => m is "public" or "private" or "protected")
                        ?? "package-private";

        // assume single variable declarator for simplicity
        var varId     = ctx.variableDeclarators()
            .variableDeclarator()[0]
            .variableDeclaratorId()
            .GetText();

        var typeText  = ctx.typeType().GetText();

        var javadoc   = JavadocExtractor.From(declCtx, tokens);

        return new AstFieldNode(
            Name      : varId,
            Type      : typeText,
            Privacy   : privacy,
            Modifiers : mods,
            Javadoc   : javadoc,
            Content   : ctx.GetText()
        );
    }
}