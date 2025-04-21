using Antlr4.Runtime;
using CodeBaseContextGenerator.Core.Interfaces.Extractors.MetaData;

namespace CodeBaseContextGenerator.Languages.JavaAntlr4.Extrators__;

public class JavaDocExtractor(CommonTokenStream tokens) : IDocExtractor<ParserRuleContext>
{
    public string? Extract(ParserRuleContext node)
    {
        if (node.Start == null || tokens == null)
            return null;

        var startIndex = node.Start.TokenIndex;

        for (int i = startIndex - 1; i >= 0; i--)
        {
            var t = tokens.Get(i);

            if (t.Channel == TokenConstants.HiddenChannel &&
                t.Text.TrimStart().StartsWith("/**"))
            {
                return t.Text.Trim();
            }

            // Stop at first non-hidden token — preserves Javadoc-only logic
            if (t.Channel != TokenConstants.HiddenChannel)
                break;
        }

        return null;
    }
}