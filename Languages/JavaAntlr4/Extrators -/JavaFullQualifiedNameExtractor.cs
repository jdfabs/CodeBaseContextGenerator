using Antlr4.Runtime;
using CodeBaseContextGenerator.Core.Interfaces.Extractors.MetaData;

namespace CodeBaseContextGenerator.Languages.JavaAntlr4.Extrators__;

public class JavaFullQualifiedNameExtractor : IFullQualifiedNameExtractor<ParserRuleContext>
{
    public string Extract(ParserRuleContext node)
    {
        var name = node switch
        {
            JavaParser.ClassDeclarationContext c => c.identifier().GetText(),
            JavaParser.InterfaceDeclarationContext i => i.identifier().GetText(),
            JavaParser.EnumDeclarationContext e => e.identifier().GetText(),
            JavaParser.RecordDeclarationContext r => r.identifier().GetText(),
            _ => throw new NotSupportedException($"Node type not supported: {node.GetType().Name}")
        };

        var compilationUnit = GetCompilationUnit(node);
        var packageName = compilationUnit?.packageDeclaration()?.qualifiedName()?.GetText();

        return string.IsNullOrEmpty(packageName) ? name : $"{packageName}.{name}";
    }

    private static JavaParser.CompilationUnitContext? GetCompilationUnit(ParserRuleContext node)
    {
        while (node is not null)
        {
            if (node is JavaParser.CompilationUnitContext cu)
                return cu;

            node = node.Parent as ParserRuleContext;
        }
        return null;
    }
}