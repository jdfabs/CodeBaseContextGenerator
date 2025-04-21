using Antlr4.Runtime;
using CodeBaseContextGenerator.Core.Interfaces.Assemblers;
using CodeBaseContextGenerator.Core.Interfaces.Representations;

namespace CodeBaseContextGenerator.Languages.JavaAntlr4.Extrators__;

public class JavaNestedTypeAssembler : INestedTypeAssembler<ParserRuleContext>
{
    private readonly ITypeRepresentationAssembler<ParserRuleContext> _typeAssembler;

    public JavaNestedTypeAssembler(ITypeRepresentationAssembler<ParserRuleContext> typeAssembler)
    {
        _typeAssembler = typeAssembler;
    }

    public IReadOnlyCollection<ITypeRepresentation> Assemble(ParserRuleContext node)
    {
        var results = new List<ITypeRepresentation>();

        // Only process nodes with class bodies
        body;
        switch (node)
        {
            case JavaParser.ClassDeclarationContext cls:
                body = cls.classBody();
                break;
            case JavaParser.InterfaceDeclarationContext iface:
                body = iface.interfaceBody();
                break;
            case JavaParser.EnumDeclarationContext enm:
                body = enm.enumBodyDeclarations();
                break;
            case JavaParser.RecordDeclarationContext rec:
                body = rec.recordBody();
                break;
            default:
                body = null;
                break;
        }

        if (body == null)
            return results;

        // Unified handling for class body declarations
        var members = body switch
        {
            JavaParser.ClassBodyContext c => c.classBodyDeclaration(),
            JavaParser.InterfaceBodyContext i => i.interfaceBodyDeclaration(),
            JavaParser.RecordBodyContext r => r.classBodyDeclaration(),
            _ => Array.Empty<ParserRuleContext>()
        };

        foreach (var member in members ?? Array.Empty<ParserRuleContext>())
        {
            // Handle only type declarations
            typeDecl;
            switch (member)
            {
                case JavaParser.ClassBodyDeclarationContext decl:
                    typeDecl = decl.memberDeclaration()?.classDeclaration();
                    break;
                case JavaParser.InterfaceBodyDeclarationContext decl:
                    typeDecl = decl.interfaceMemberDeclaration()?.interfaceDeclaration();
                    break;
                default:
                    typeDecl = null;
                    break;
            }

            if (typeDecl != null)
            {
                var nested = _typeAssembler.Assemble(typeDecl);
                results.Add(nested);
            }
        }

        return results;
    }
}
