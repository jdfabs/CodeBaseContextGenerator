using Antlr4.Runtime;
using CodeBaseContextGenerator.Core.Interfaces.Extractors.MetaData;

namespace CodeBaseContextGenerator.Languages.JavaAntlr4.Extrators__;

public class JavaNameExtractor : INameExtractor<ParserRuleContext>
{
    public string Extract(ParserRuleContext node)
    {
        return node switch
        {
            JavaParser.ClassDeclarationContext cls => cls.identifier().GetText(),
            JavaParser.InterfaceDeclarationContext iface => iface.identifier().GetText(),
            JavaParser.EnumDeclarationContext enm => enm.identifier().GetText(),
            JavaParser.RecordDeclarationContext rec => rec.identifier().GetText(),
            _ => throw new NotImplementedException($"Unsupported node type: {node.GetType().Name}")
        };
    }
}

