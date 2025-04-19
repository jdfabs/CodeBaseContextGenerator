using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace CodeBaseContextGenerator.JavaAntlr4;

public class TypeUsageCollector : JavaParserBaseVisitor<List<TypeReference>>
{
    private readonly string _rootPath;
    private readonly string _sourcePath;
    private readonly HashSet<string> _seen = new();

    public TypeUsageCollector(string rootPath, string sourcePath)
    {
        _sourcePath = sourcePath;
        _rootPath = rootPath;
    }

    public override List<TypeReference> VisitCreator(JavaParser.CreatorContext context)
    {
        // Match: new SomeType()
        var identifier = context.createdName()?.identifier(0)?.GetText();
        AddIfValid(identifier, "uses");
        return base.VisitCreator(context);
    }

    public List<TypeReference> VisitExpression(JavaParser.ExpressionContext context)
    {
        var text = context.GetText();

        // Try to catch patterns like: SomeType.something
        var parts = text.Split('.');
        if (parts.Length >= 2)
        {
            var first = parts[0];
            if (!string.IsNullOrEmpty(first) && char.IsUpper(first[0]))
            {
                AddIfValid(first, "uses");
            }
        }

        return base.VisitChildren(context);
    }


    public override List<TypeReference> VisitParExpression(JavaParser.ParExpressionContext context)
    {
        // Inside parentheses — may wrap a cast like: (SomeType)obj
        return base.VisitParExpression(context);
    }

    public override List<TypeReference> VisitTypeType(JavaParser.TypeTypeContext context)
    {
        // Match: (SomeType) or field declarations
        var identifier = context.classOrInterfaceType()?.identifier(0)?.GetText();
        AddIfValid(identifier, "uses");
        return base.VisitTypeType(context);
    }

    private void AddIfValid(string? typeName, string kind)
    {
        if (string.IsNullOrWhiteSpace(typeName)) return;
        if (!_seen.Add(typeName)) return; // avoid duplicates

        _results.Add(new TypeReference
        {
            Name = typeName,
            Source = $"{typeName}@{Path.GetRelativePath(_rootPath,_sourcePath)}",
            Kind = kind
        });
    }

    private readonly List<TypeReference> _results = new();
    public override List<TypeReference> VisitChildren(IRuleNode node)
    {
        foreach (var child in Enumerable.Range(0, node.ChildCount).Select(node.GetChild))
        {
            if (child is ParserRuleContext ctx)
                Visit(ctx);
        }
        return _results;
    }
}
