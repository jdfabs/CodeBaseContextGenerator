namespace CodeBaseContextGenerator.JavaAntlr4.Extrators;

internal static class InheritanceExtractor
{
    public static IReadOnlyCollection<TypeReference> From(JavaParser.ClassDeclarationContext ctx)
    {
        var list = new List<TypeReference>();

        if (ctx.EXTENDS() != null && ctx.typeType() != null)
            list.Add(new TypeReference { Name = ctx.typeType().GetText(), Kind = "extends" });

        if (ctx.IMPLEMENTS() != null && ctx.typeList() != null)
        {
            foreach (var ifaceList in ctx.typeList())
            {
                foreach (var child in ifaceList.children)
                {
                    var id = child.GetText();
                    if (id == ",") continue;
                    list.Add(new TypeReference { Name = id, Kind = "implements" });
                }
            }
        }

        return list;
    }

    public static IReadOnlyCollection<TypeReference> From(JavaParser.InterfaceDeclarationContext ctx)
    {
        var list = new List<TypeReference>();

        if (ctx.EXTENDS() != null && ctx.typeList() != null)
        {
            foreach (var iface in ctx.typeList())
                list.Add(new TypeReference { Name = iface.GetText(), Kind = "implements" });
        }

        return list;
    }
}