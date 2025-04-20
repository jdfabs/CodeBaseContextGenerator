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

        string parameters = FormatParameters(ctx);

        return new AstMethodNode(
            Name: ctx.identifier()?.GetText() ?? "<unknown>",
            Privacy: privacy,
            ReturnType: ctx.typeTypeOrVoid()?.GetText() ?? "void",
            Parameters: parameters,
            Content: ctx.GetText(),
            ReferencedTypes: referenced
        );
    }
    
    private static string FormatParameters(JavaParser.MethodDeclarationContext ctx)
    {
        var paramList = ctx.formalParameters()?.formalParameterList();
        if (paramList == null || paramList.formalParameter().Length == 0)
            return "()";

        var formatted = paramList
            .formalParameter()
            .Select(param =>
            {
                var type = param.typeType()?.GetText() ?? "unknown";
                var name = param.variableDeclaratorId()?.GetText() ?? "arg";
                return $"{type} {name}";
            });

        return $"({string.Join(", ", formatted)})";
    }
}