using Antlr4.Runtime;
using CodeBaseContextGenerator.Core.Interfaces.Extractors.Structure;

namespace CodeBaseContextGenerator.Languages.JavaAntlr4.Extrators__;

public class JavaCodeExtractor : ICodeExtractor<ParserRuleContext>
{
    public string Extract(ParserRuleContext node)
    {
       return node.GetText(); 
    }
}