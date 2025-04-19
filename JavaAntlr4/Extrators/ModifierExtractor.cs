namespace CodeBaseContextGenerator.JavaAntlr4.Extrators;

internal static class ModifierExtractor
{
    public static IReadOnlyCollection<string> From(JavaParser.ClassDeclarationContext ctx) =>
        (ctx.Parent as JavaParser.TypeDeclarationContext)?.classOrInterfaceModifier()
        .Select(m => m.GetText())
        .ToArray() ?? System.Array.Empty<string>();

    public static IReadOnlyCollection<string> From(JavaParser.InterfaceDeclarationContext ctx) =>
        (ctx.Parent as JavaParser.TypeDeclarationContext)?.classOrInterfaceModifier()
        .Select(m => m.GetText())
        .ToArray() ?? System.Array.Empty<string>();

    public static IReadOnlyCollection<string> From(JavaParser.MethodDeclarationContext ctx) =>
        (ctx.Parent?.Parent as JavaParser.ClassBodyDeclarationContext)?.modifier()
        .Select(m => m.GetText())
        .ToArray() ?? System.Array.Empty<string>();
}