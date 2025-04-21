using System.Collections.Immutable;
using Antlr4.Runtime;
using CodeBaseContextGenerator.Core.Interfaces.Extractors.MetaData;
using CodeBaseContextGenerator.Core.Interfaces.Representations.Properties;

namespace CodeBaseContextGenerator.Languages.JavaAntlr4.Extrators__;

public class JavaModifierExtractor : IModifierExtractor<ParserRuleContext>
{
    public ImmutableHashSet<CodeModifier> Extract(ParserRuleContext node)
    {
        var rawModifiers = node switch
        {
            JavaParser.ClassDeclarationContext cls =>
                (cls.Parent as JavaParser.TypeDeclarationContext)?
                    .classOrInterfaceModifier()
                    .Select(m => m.GetText()),

            JavaParser.InterfaceDeclarationContext iface =>
                (iface.Parent as JavaParser.TypeDeclarationContext)?
                    .classOrInterfaceModifier()
                    .Select(m => m.GetText()),

            JavaParser.MethodDeclarationContext method =>
                (method.Parent?.Parent as JavaParser.ClassBodyDeclarationContext)?
                    .modifier()
                    .Select(m => m.GetText()),

            JavaParser.ConstructorDeclarationContext ctor =>
                (ctor.Parent?.Parent as JavaParser.ClassBodyDeclarationContext)?
                    .modifier()
                    .Select(m => m.GetText()),

            JavaParser.ClassBodyDeclarationContext decl =>
                decl.modifier().Select(m => m.GetText()),

            _ => throw new NotImplementedException()
        };

        return Normalize(rawModifiers);
    }

    private static ImmutableHashSet<CodeModifier> Normalize(IEnumerable<string>? raw)
    {
        if (raw == null) return ImmutableHashSet<CodeModifier>.Empty;
        
        return raw
            .Select(m => m.ToLowerInvariant())
            .Select(m => m switch
            {
                "public" => CodeModifier.Public,
                "private" => CodeModifier.Private,
                "protected" => CodeModifier.Protected,
                "static" => CodeModifier.Static,
                "abstract" => CodeModifier.Abstract,
                "final" => CodeModifier.Sealed,     // map Java `final` to .NET `Sealed` if conceptually similar
                "readonly" => CodeModifier.ReadOnly,
                "const" => CodeModifier.Const,
                _ => (CodeModifier?)null
            })
            .Where(m => m.HasValue)
            .Select(m => m!.Value)
            .ToImmutableHashSet();
    }
}
