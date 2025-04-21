using Antlr4.Runtime;
using CodeBaseContextGenerator.JavaAntlr4.Builders;
using CodeBaseContextGenerator.JavaAntlr4.Extrators;

namespace CodeBaseContextGenerator.JavaAntlr4.Visitors;

internal static class MethodVisitor
{
    public static AstMethodNode Build(JavaParser.MethodDeclarationContext ctx, string root, string src, CommonTokenStream tokens)
    {
        var modifiers = ModifierExtractor.From(ctx);
        var referenced = new TypeUsageCollector(root, src).Visit(ctx.methodBody());

        string parameters = FormatParameters(ctx);
        
        // throws clause
        var throwsList = ctx.qualifiedNameList()?.qualifiedName()
            .Select(q => q.GetText())
            .ToList();
        var throwsArr = throwsList?.Any() == true
            ? string.Join(", ", throwsList)
            : null;

        return new AstMethodNode(
            Name: ctx.identifier()?.GetText() ?? "<unknown>",
            Modifiers: modifiers,
            ReturnType: ctx.typeTypeOrVoid()?.GetText() ?? "void",
            Parameters: parameters,
            Content: ctx.GetText(),
            ReferencedTypes: referenced,
            Javadoc: JavadocExtractor.From(ctx,tokens) ?? string.Empty,
            Throws:  throwsArr != null ? string.Join(", ", throwsArr) : String.Empty 
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