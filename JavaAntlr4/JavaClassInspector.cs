using Antlr4.Runtime;
using Antlr4.Runtime.Misc;

namespace CodeBaseContextGenerator.JavaAntlr4;
public class JavaClassInspector : JavaParserBaseVisitor<List<TypeRepresentation>>
{
    private readonly string _rootPath;
    private readonly string _sourcePath;
    private readonly List<TypeRepresentation> _items = new();
    private string _currentTypeName = "";
    private TypeRepresentation? _currentClass;
    public JavaClassInspector(string fullSourcePath, string rootPath)
    {
        _sourcePath = fullSourcePath;
        _rootPath = rootPath;
    }

    public override List<TypeRepresentation> VisitCompilationUnit(JavaParser.CompilationUnitContext context)
    {
        foreach (var typeDecl in context.typeDeclaration())
            Visit(typeDecl);

        return _items;
    }

    public override List<TypeRepresentation> VisitTypeDeclaration(JavaParser.TypeDeclarationContext ctx)
    {
        if (ctx.classDeclaration() != null)
            Visit(ctx.classDeclaration());
        else if (ctx.interfaceDeclaration() != null)
            Visit(ctx.interfaceDeclaration());

        return _items;
    }

    public override List<TypeRepresentation> VisitClassDeclaration(JavaParser.ClassDeclarationContext ctx)
    {
        var prevType = _currentTypeName;
        var prevClass = _currentClass;

        _currentTypeName = ctx.identifier()?.GetText() ?? "<unknown-class>";

        var modifiers = ctx.Parent?.Parent is JavaParser.TypeDeclarationContext td && td.classOrInterfaceModifier().Any()
            ? td.classOrInterfaceModifier().Select(m => m.GetText()).ToArray()
            : Array.Empty<string>();

        var classRep = new TypeRepresentation
        {
            Name = _currentTypeName,
            Type = "class",
            Privacy = ExtractPrivacy(modifiers),
            SourcePath = Path.GetRelativePath(_rootPath, _sourcePath),
            Content = GetOriginalText(ctx),
            ReferencedTypes = ExtractExtendsImplements(ctx),
            Methods = new List<TypeRepresentation>()
        };

        _currentClass = classRep;

        foreach (var member in ctx.classBody().classBodyDeclaration())
            Visit(member);

        _items.Add(classRep);

        _currentClass = prevClass;
        _currentTypeName = prevType;

        return _items;
    }

    public override List<TypeRepresentation> VisitInterfaceDeclaration(JavaParser.InterfaceDeclarationContext ctx)
    {
        var prevType = _currentTypeName;
        _currentTypeName = ctx.identifier()?.GetText() ?? "<unknown-interface>";

        var modifiers = ctx.Parent.Parent is JavaParser.TypeDeclarationContext td && td.classOrInterfaceModifier().Any()
            ? td.classOrInterfaceModifier().Select(m => m.GetText()).ToArray()
            : Array.Empty<string>();

        var ifaceType = new TypeRepresentation
        {
            Name = _currentTypeName,
            Type = "interface",
            Privacy = ExtractPrivacy(modifiers),
            SourcePath = Path.GetRelativePath(_rootPath, _sourcePath),
            Content = GetOriginalText(ctx),
            ReferencedTypes = ExtractExtendsInterfaces(ctx)
        };

        _items.Add(ifaceType);

        foreach (var member in ctx.interfaceBody()?.interfaceBodyDeclaration() ?? [])
            Visit(member);

        _currentTypeName = prevType;
        return _items;
    }

    public override List<TypeRepresentation> VisitClassBodyDeclaration(JavaParser.ClassBodyDeclarationContext ctx)
    {
        if (ctx.memberDeclaration() != null)
            Visit(ctx.memberDeclaration());
        return _items;
    }

    public override List<TypeRepresentation> VisitMemberDeclaration(JavaParser.MemberDeclarationContext ctx)
    {
        if (ctx.methodDeclaration() != null)
            Visit(ctx.methodDeclaration());

        return _items;
    }

    public override List<TypeRepresentation> VisitMethodDeclaration(JavaParser.MethodDeclarationContext ctx)
    {
        var returnType = ctx.typeTypeOrVoid()?.GetText() ?? "void";
        var methodName = ctx.identifier()?.GetText() ?? "<unknown>";
        var paramList = ctx.formalParameters()?.GetText() ?? "()";
        var block = GetOriginalText(ctx) ?? "{}";

        var modifiers = ctx.Parent?.Parent is JavaParser.ClassBodyDeclarationContext cbd
            ? cbd.modifier().Select(m => m.GetText()).ToArray()
            : Array.Empty<string>();
        var referencedTypes = new TypeUsageCollector(_rootPath,_sourcePath).Visit(ctx.methodBody());

        var method = new TypeRepresentation
        {
            Name = $"{_currentTypeName}.{methodName}",
            Type = "method",
            Privacy = ExtractPrivacy(modifiers),
            ReturnType = returnType,
            Parameters = paramList,
            Content = block,
            SourcePath = Path.GetRelativePath(_rootPath, _sourcePath),
            ReferencedTypes = referencedTypes
        };;

        // 🔧 NEW: Attach to parent class if available
        if (_currentClass != null)
        {
            _currentClass.Methods ??= new List<TypeRepresentation>();
            _currentClass.Methods.Add(method);
        }
        else
        {
            // fallback: store as top-level method (shouldn’t happen)
            _items.Add(method);
        }

        return _items;
    }

    private string ExtractPrivacy(string[] modifiers)
    {
        if (modifiers.Contains("public")) return "public";
        if (modifiers.Contains("protected")) return "protected";
        if (modifiers.Contains("private")) return "private";
        return "package-private";
    }

    private List<TypeReference> ExtractExtendsImplements(JavaParser.ClassDeclarationContext ctx)
    {
        var references = new List<TypeReference>();

        // Extract 'extends' single superclass
        if (ctx.EXTENDS() != null && ctx.typeType() != null)
        {
            var baseType = ctx.typeType().GetText();
            references.Add(new TypeReference
            {
                Name = baseType,
                Source = $"{baseType}@{Path.GetRelativePath(_rootPath, _sourcePath)}",
                Kind = "extends"
            });
        }

        // Extract 'implements' multiple interfaces properly
        if (ctx.IMPLEMENTS() != null && ctx.typeList() != null)
        {
            var interfaces = ctx.typeList(); // <- this is the fix
            foreach (var iface in interfaces)
            {
                foreach (var child in iface.children)
                {
                    if(child.GetText()== ",") continue;
                    references.Add(new TypeReference
                    {
                        Name = child.GetText(),
                        Source = $"{child.GetText()}@{Path.GetRelativePath(_rootPath, _sourcePath)}",
                        Kind = "implements"
                    });
                }
            }
        }

        return references;
    }


    private List<TypeReference> ExtractExtendsInterfaces(JavaParser.InterfaceDeclarationContext ctx)
    {
        var references = new List<TypeReference>();
        if (ctx.EXTENDS() != null && ctx.typeList() != null)
        {
            foreach (var iface in ctx.typeList()) // ✅
            {
                var ifaceName = iface.GetText(); // ✅ Safe
                references.Add(new TypeReference
                {
                    Name = ifaceName,
                    Source = $"{ifaceName}@{Path.GetRelativePath(_rootPath, _sourcePath)}", // ✅ Clean
                    Kind = "implements"
                });
            }
        }
        return references;
    }

    private List<TypeReference> FindReferencedTypes(string body)
    {
        // Placeholder: you could tokenize body or treewalk to detect usages
        return new List<TypeReference>();
    }
    
    private string GetOriginalText(ParserRuleContext ctx)
    {
        var start = ctx.Start.StartIndex;
        var stop = ctx.Stop.StopIndex;
        var interval = new Interval(start, stop);
        return ctx.Start.InputStream.GetText(interval);
    }
}
