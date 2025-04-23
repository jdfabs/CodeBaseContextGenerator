using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using CodeBaseContextGenerator.Core.Interfaces.Extractors.Relationship;
using CodeBaseContextGenerator.Core.Interfaces.Representations;
using CodeBaseContextGenerator.Core.Models.Representations;

namespace CodeBaseContextGenerator.Languages.JavaAntlr4.Extrators__;

public class JavaReferenceTypeExtractor : ITypeUsageExtractor<ParserRuleContext>
{
    public IReadOnlyCollection<ITypeReference> Extract(ParserRuleContext node)
    {
        var collector = new InternalTypeUsageCollector(node);
        collector.Visit(node);
        return collector.Results;
    }

    private class InternalTypeUsageCollector(ParserRuleContext node) : JavaParserBaseVisitor<object>
    {
        private readonly HashSet<string> _seen = new();
        private readonly List<ITypeReference> _results = new();

        public IReadOnlyCollection<ITypeReference> Results => _results;

        public override object VisitCreator(JavaParser.CreatorContext context)
        {
            var typeName = context.createdName()?.identifier(0)?.GetText();
            AddIfValid(typeName, ReferenceKind.Uses);
            return base.VisitChildren(context);
        }

        public override object VisitTypeType(JavaParser.TypeTypeContext context)
        {
            var identifier = context.classOrInterfaceType()?.identifier(0)?.GetText();
            AddIfValid(identifier, ReferenceKind.Uses);
            return base.VisitChildren(context);
        }

        public override object VisitClassOrInterfaceType(JavaParser.ClassOrInterfaceTypeContext context)
        {
            var identifier = context.identifier(0)?.GetText();
            AddIfValid(identifier, ReferenceKind.Uses);
            return base.VisitChildren(context);
        }

        private void AddIfValid(string? typeName, ReferenceKind kind)
        {
            if (string.IsNullOrWhiteSpace(typeName)) return;
            if (!_seen.Add(typeName)) return;

            /*_results.Add(new TypeReferenceBase
            {
                Name = typeName,
                Kind = kind,
                Source = "", // can be resolved later via FQN resolver
                FullQualifiedName = new JavaFullQualifiedNameExtractor().Extract(node),
                Type = new JavaFullQualifiedNameExtractor().Extract(node)
            });*/
        }
    }

    public IReadOnlyCollection<ITypeReference> ExtractReferences(ParserRuleContext node)
    {
        throw new NotImplementedException();
    }
}
