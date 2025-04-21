using Antlr4.Runtime;
using CodeBaseContextGenerator.Core.Interfaces.Extractors.MetaData;

namespace CodeBaseContextGenerator.Languages.JavaAntlr4.Extrators__;

public class JavaKindExtractor : ITypeExtractor<ParserRuleContext>
{
    public string Extract(ParserRuleContext node)
    {
        return node switch
        {
            JavaParser.ClassDeclarationContext => "class",
            JavaParser.InterfaceDeclarationContext => "interface",
            JavaParser.EnumDeclarationContext => "enum",
            JavaParser.RecordDeclarationContext => "record",
            _ => throw new NotSupportedException($"Unsupported type node: {node.GetType().Name}")
        };
    }
}