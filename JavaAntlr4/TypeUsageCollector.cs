using Antlr4.Runtime;
using Antlr4.Runtime.Tree;

namespace CodeBaseContextGenerator.JavaAntlr4;

/// <summary>
/// Gathers referenced types within method bodies, including detection
/// of thrown exceptions to mark them with "throws" instead of "uses".
/// </summary>
public sealed class TypeUsageCollector : JavaParserBaseVisitor<List<TypeReference>>
{
    private readonly string _rootPath;
    private readonly string _sourcePath;
    private readonly HashSet<string> _seen = new();
    private readonly List<TypeReference> _results = new();

    public TypeUsageCollector(string rootPath, string sourcePath)
    {
        _rootPath = rootPath;
        _sourcePath = sourcePath;
    }

    public override List<TypeReference> VisitCreator(JavaParser.CreatorContext context)
    {
        var typeName = context.createdName()?.identifier(0)?.GetText();
        AddIfValid(typeName, "uses");
        return base.VisitChildren(context);
    }

    public override List<TypeReference> VisitTypeType(JavaParser.TypeTypeContext context)
    {
        // Field declarations, casts, etc.
        var identifier = context.classOrInterfaceType()?.identifier(0)?.GetText();
        AddIfValid(identifier, "uses");
        return base.VisitChildren(context);
    }

    public override List<TypeReference> VisitChildren(IRuleNode node)
    {
        foreach (var child in Enumerable.Range(0, node.ChildCount)
                     .Select(node.GetChild)
                     .OfType<IRuleNode>())
        {
            Visit(child);
        }

        return _results;
    }

    private void AddIfValid(string? typeName, string kind)
    {
        if (string.IsNullOrWhiteSpace(typeName)) return;
        if (!_seen.Add(typeName)) return;

        var rel = Path.GetRelativePath(_rootPath, _sourcePath).Replace('\\', '/');
        _results.Add(new TypeReference
        {
            Name = typeName,
            Kind = kind,
            Source = $"{typeName}@{rel}"
        });
    }

}