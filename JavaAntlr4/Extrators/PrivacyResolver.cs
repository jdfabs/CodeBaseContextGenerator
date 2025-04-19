using Antlr4.Runtime;

namespace CodeBaseContextGenerator.JavaAntlr4.Extrators;

internal static class PrivacyResolver
{
    public static string From(ParserRuleContext ctx)
    {
        var mods = ctx.GetRuleContexts<JavaParser.ModifierContext>()
            .Select(m => m.GetText())
            .ToArray();

        if (mods.Contains("public"))    return "public";
        if (mods.Contains("protected")) return "protected";
        if (mods.Contains("private"))   return "private";
        return "package-private";
    }
}