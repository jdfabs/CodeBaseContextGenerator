using System.Collections.Immutable;
using Antlr4.Runtime;
using CodeBaseContextGenerator.Core.Interfaces.Extractors.Structure;
using CodeBaseContextGenerator.Core.Interfaces.Representations;
using CodeBaseContextGenerator.Core.Interfaces.Representations.Properties;
using CodeBaseContextGenerator.Core.Models.Representations;
using CodeBaseContextGenerator.Languages.JavaAntlr4.Extrators__;

public class JavaFieldExtractor : IFieldExtractor<ParserRuleContext>
{
    public IReadOnlyCollection<IFieldRepresentation> Extract(ParserRuleContext node)
    {
        if (node is not JavaParser.FieldDeclarationContext fieldDecl)
            return Array.Empty<IFieldRepresentation>();

        var result = new List<IFieldRepresentation>();

        // field type
        var typeText = fieldDecl.typeType().GetText();
        var typeRef = new TypeReferenceBase
        {
            Name = typeText,
            Kind = ReferenceKind.Uses,
            Source = "", // can be resolved later
            FullQualifiedName = null,
            Type = null
        };

        // parent context has modifiers
        var declCtx = fieldDecl.Parent?.Parent as JavaParser.ClassBodyDeclarationContext;
        var rawModifiers = declCtx?.modifier()
            .Select(m => m.GetText().ToLowerInvariant())
            .ToArray() ?? Array.Empty<string>();

        var privacy = rawModifiers.FirstOrDefault(m => m is "public" or "private" or "protected")
            ?? "package-private";

        var modifiers = rawModifiers
            .Select(m => m switch
            {
                "public" => CodeModifier.Public,
                "private" => CodeModifier.Private,
                "protected" => CodeModifier.Protected,
                "static" => CodeModifier.Static,
                "final" => CodeModifier.Sealed,
                _ => (CodeModifier?)null
            })
            .Where(m => m.HasValue)
            .Select(m => m!.Value)
            .ToImmutableHashSet();

        // javadoc: optional, for now assume null — inject later if needed
        string? javadoc = null;

        // handle multiple field declarations in one line
        foreach (var declarator in fieldDecl.variableDeclarators().variableDeclarator())
        {
            var name = declarator.variableDeclaratorId().GetText();
            var field = new FieldRepresentationBase
            {
                Name = name,
                Type = typeRef,
                Modifiers = modifiers,
                Docs = javadoc,
                FullQualifiedName = new JavaFullQualifiedNameExtractor().Extract(node)
            };

            result.Add(field);
        }

        return result;
    }
}
