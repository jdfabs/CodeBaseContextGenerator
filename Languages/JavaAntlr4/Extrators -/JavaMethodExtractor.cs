using Antlr4.Runtime;
using CodeBaseContextGenerator.Core.Interfaces.Extractors.Structure;
using CodeBaseContextGenerator.Core.Interfaces.Representations;
using CodeBaseContextGenerator.Core.Models.Representations;

namespace CodeBaseContextGenerator.Languages.JavaAntlr4.Extrators__;

public class JavaMethodExtractor(CommonTokenStream tokenStream): IMethodExtractor<ParserRuleContext>
{
    private JavaDocExtractor _docExtractor = new(tokenStream);
    private JavaModifierExtractor _modifierExtractor = new();
    private JavaRefecenceTypeExtractor _usageCollector = new();
    private JavaCodeExtractor _codeExtractor = new();

    public IReadOnlyCollection<IMethodRepresentation> Extract(ParserRuleContext node)
    {
        if (node is not JavaParser.MethodDeclarationContext method)
            return Array.Empty<IMethodRepresentation>();

        var declCtx = method.Parent?.Parent as JavaParser.ClassBodyDeclarationContext;

        // Modifiers + privacy
        var rawModifiers = declCtx?.modifier()
            .Select(m => m.GetText().ToLowerInvariant())
            .ToArray() ?? Array.Empty<string>();

        var privacy = rawModifiers.FirstOrDefault(m => m is "public" or "private" or "protected")
                      ?? "package-private";

        var modifiers = _modifierExtractor.Extract(method);
        var referencedTypes = _usageCollector.Extract(method);

        // Return type
        var returnTypeName = method.typeTypeOrVoid()?.GetText() ?? "void";
        var returnType = new TypeReferenceBase
        {
            Name = returnTypeName,
            Kind = ReferenceKind.Uses,
            Source = "",
            FullQualifiedName = null,
            Type = null
        };

        // Parameters (structured)
        var parameters = method.formalParameters()
            ?.formalParameterList()
            ?.formalParameter()
            ?.Select(p =>
            {
                var type = p.typeType()?.GetText() ?? "Object";
                var name = p.variableDeclaratorId()?.GetText() ?? "arg";
                return new Parameter(
                    new TypeReferenceBase
                    {
                        Name = type,
                        Kind = ReferenceKind.Uses,
                        Source = "",
                        FullQualifiedName = null,
                        Type = null
                    },
                    name
                );
            })
            .ToList() ?? new List<Parameter>();

        // Exceptions (throws clause)
        var throws = method.qualifiedNameList()?.qualifiedName()
            .Select(q => q.GetText())
            .ToList() ?? new List<string>();

        // Javadoc and raw code
        var docs = _docExtractor.Extract(method);
        var code = _codeExtractor.Extract(method);

        var methodRep = new MethodRepresentationBase
        {
            Name = method.identifier()
                       ?.GetText() ??
                   "<unknown>",
            ReturnType = returnType,
            Modifiers = modifiers,
            Parameters = parameters,
            ExceptionsThrown = throws,
            Docs = docs,
            Code = code,
            ReferenceTypes = referencedTypes,
            FullQualifiedName = new JavaFullQualifiedNameExtractor().Extract(node),
            FilePath = new JavaFullQualifiedNameExtractor().Extract(node),
        };

        return new[] { methodRep };
    }
}