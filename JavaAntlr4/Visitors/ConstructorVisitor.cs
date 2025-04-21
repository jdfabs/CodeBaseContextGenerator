using Antlr4.Runtime;
using CodeBaseContextGenerator.JavaAntlr4.Builders;
using CodeBaseContextGenerator.JavaAntlr4.Extrators;

namespace CodeBaseContextGenerator.JavaAntlr4.Visitors;

public class ConstructorVisitor
{
    public static AstConstructorNode Build(
        JavaParser.ConstructorDeclarationContext ctx,
        CommonTokenStream tokens)
    {
        // the parent is ClassBodyDeclarationContext
        var declCtx = ctx.Parent.Parent as JavaParser.ClassBodyDeclarationContext;
        var mods = ModifierExtractor.From(declCtx);
        var privacy = mods.FirstOrDefault(m => m is "public" or "private" or "protected")
                      ?? "package-private";

        // parameters list (possibly empty)
        var paramsText = ctx.formalParameters()?.formalParameterList().formalParameter()
            .Select(p =>
            {
                var type = p.typeType()?.GetText() ?? "Object";
                var name = p.variableDeclaratorId().GetText() ?? "param";
                return $"{type} {name}";
            });
        
        var finalParams = paramsText != null
            ? $"({string.Join(", ", paramsText)})"
            : "()";

        // throws clause
        var throwsList = ctx.qualifiedNameList()?.qualifiedName()
            .Select(q => q.GetText())
            .ToList();
        var throwsArr = throwsList?.Any() == true
            ? string.Join(", ", throwsList)
            : null;


        var javadoc = JavadocExtractor.From(declCtx, tokens);

        return new AstConstructorNode(
            Name: ctx.identifier().GetText(),
            Privacy: privacy,
            Modifiers: mods,
            Parameters: finalParams,
            Throws: throwsArr != null ? string.Join(", ", throwsArr) : null,
            Javadoc: javadoc,
            Content: ctx.GetText()
        );
    }
}