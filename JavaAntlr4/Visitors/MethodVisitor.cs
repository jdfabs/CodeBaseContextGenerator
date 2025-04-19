using CodeBaseContextGenerator.JavaAntlr4.Builders;
using CodeBaseContextGenerator.JavaAntlr4.Extrators;

namespace CodeBaseContextGenerator.JavaAntlr4.Visitors;

internal static class MethodVisitor
{
    public static AstMethodNode Build(JavaParser.MethodDeclarationContext ctx, string root, string src)
    {
        var mods = ModifierExtractor.From(ctx);
        var privacy = mods.FirstOrDefault(m => m is "public" or "private" or "protected") ?? "package-private";

        var referenced = new TypeUsageCollector(root, src).Visit(ctx.methodBody());

        return new AstMethodNode(
            Name:       ctx.identifier().GetText(),
            Privacy:    privacy,
            ReturnType: ctx.typeTypeOrVoid()?.GetText() ?? "void",
            Parameters: ctx.formalParameters()?.GetText() ?? "()",
            Content:    ctx.GetText(),
            ReferencedTypes: referenced
        );
    }
}