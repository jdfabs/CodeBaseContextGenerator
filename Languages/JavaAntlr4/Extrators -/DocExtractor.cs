using Antlr4.Runtime;

namespace CodeBaseContextGenerator.JavaAntlr4.Extrators;

public static class DocExtractor
{
    /// <summary>
    /// Scans backward from the start of ctx for a hidden‑channel token
    /// whose text begins with "/**". Returns the first-match Javadoc comment.
    /// </summary>
    public static string? From(ParserRuleContext ctx, CommonTokenStream tokens)
    {
        var tokenStream = tokens;
        var startIndex   = ctx.Start.TokenIndex;

        for (int i = startIndex - 1; i >= 0; i--)
        {
            var t = tokenStream.Get(i);
            if (t.Channel == TokenConstants.HiddenChannel
                && t.Text.TrimStart().StartsWith("/**"))
            {
                return t.Text.Trim();
            }
            // stop at any non‑hidden token
            if (t.Channel == TokenConstants.HiddenChannel)
                break;
        }
        return null;
    }
}