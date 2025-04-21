using Antlr4.Runtime;
using CodeBaseContextGenerator.Core.Interfaces.Extractors.Relationship;
using CodeBaseContextGenerator.Core.Interfaces.Representations;
using CodeBaseContextGenerator.Core.Models.Representations;

namespace CodeBaseContextGenerator.Languages.JavaAntlr4.Extrators__;

public class JavaBaseTypeExtractor : IBaseTypeExtractor<ParserRuleContext>
{
    public IReadOnlyCollection<ITypeReference> Extract(ParserRuleContext node)
    {
        var results = new List<ITypeReference>();

        switch (node)
        {
            case JavaParser.ClassDeclarationContext cls:
                {
                    if (cls.EXTENDS() != null && cls.typeType() is { } baseType)
                    {
                        results.Add(Create(baseType.GetText(), ReferenceKind.Extends, node));
                    }

                    if (cls.IMPLEMENTS() != null && cls.typeList()?.ToList() is { } interfaces)
                    {
                        foreach (var intf in interfaces)
                        {
                            results.Add(Create(intf.GetText(), ReferenceKind.Implements, node));
                        }
                    }

                    break;
                }

            case JavaParser.InterfaceDeclarationContext iface:
                {
                    if (iface.EXTENDS() != null && iface.typeList()?.ToList() is { } interfaces)
                    {
                        foreach (var intf in interfaces)
                        {
                            results.Add(Create(intf.GetText(), ReferenceKind.Extends, node));
                        }
                    }

                    break;
                }

            case JavaParser.EnumDeclarationContext enm:
                {
                    if (enm.IMPLEMENTS() != null && enm.typeList()?.typeType() is { } interfaces)
                    {
                        foreach (var intf in interfaces)
                        {
                            results.Add(Create(intf.GetText(), ReferenceKind.Implements, node));
                        }
                    }

                    break;
                }
        }

        return results;
    }

    private static ITypeReference Create(string name, ReferenceKind kind, ParserRuleContext context)
    {
        var fqn = new JavaFullQualifiedNameExtractor().Extract(context);
        return new TypeReferenceBase
        {
            Name = name,
            Kind = kind,
            Source = $"{name}@{fqn}", // basic source anchor
            FullQualifiedName = null, // optional target resolution
            Type = null // optional resolved target
        };
    }
}
