using System.Collections.Immutable;
using Antlr4.Runtime;
using CodeBaseContextGenerator.Core.Interfaces.Extractors.Structure;
using CodeBaseContextGenerator.Core.Interfaces.Representations;
using CodeBaseContextGenerator.Core.Interfaces.Representations.Properties;
using CodeBaseContextGenerator.Core.Models.Representations;

namespace CodeBaseContextGenerator.Languages.JavaAntlr4.Extrators__;

public class JavaConstructorExtractor : IConstructorExtractor<ParserRuleContext>
{
    public IReadOnlyCollection<IConstructorRepresentation> Extract(ParserRuleContext node)
    {
        if (node is not JavaParser.ConstructorDeclarationContext ctor)
            return Array.Empty<IConstructorRepresentation>();

        var declCtx = ctor.Parent?.Parent as JavaParser.ClassBodyDeclarationContext;

        // Modifiers
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
                "abstract" => CodeModifier.Abstract,
                _ => (CodeModifier?)null
            })
            .Where(m => m.HasValue)
            .Select(m => m!.Value)
            .ToImmutableHashSet();

        // Parameters
        var paramList = ctor.formalParameters()?.formalParameterList()?.formalParameter()
            .Select(p =>
            {
                var type = p.typeType()?.GetText() ?? "Object";
                var name = p.variableDeclaratorId()?.GetText() ?? "param";
                return $"{type} {name}";
            });

        var parameters = ctor.formalParameters()
            ?.formalParameterList()
            ?.formalParameter()
            ?.Select(p =>
            {
                var typeName = p.typeType()?.GetText() ?? "Object";
                var name = p.variableDeclaratorId()?.GetText() ?? "param";

                return new Parameter(
                    new TypeReferenceBase
                    {
                        Name = typeName,
                        Kind = ReferenceKind.Uses,
                        Source = "",
                        FullQualifiedName = new JavaFullQualifiedNameExtractor().Extract(node),
                        Type = "var",
                    },
                    name
                );
            })
            .ToList() ?? new List<Parameter>();

        // Exceptions
        var throws = ctor.qualifiedNameList()?.qualifiedName()
            .Select(q => q.GetText())
            .ToList() ?? new List<string>();

        // Javadoc — optional (null for now)
        string? javadoc = null;

        var constructor = new ConstructorRepresentationBase
        {
            Name = ctor.identifier()
                       ?.GetText() ??
                   "<unknown>",
            Modifiers = modifiers,
            Parameters = parameters,
            ExceptionsThrown = throws,
            Docs = javadoc,
            FullQualifiedName = new JavaFullQualifiedNameExtractor().Extract(node),
            Code = new JavaCodeExtractor().Extract(node) ,
            FilePath = new JavaFullQualifiedNameExtractor().Extract(node) 
        };

        return new[] { constructor };
    }
}