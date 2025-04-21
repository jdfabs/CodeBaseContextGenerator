using Antlr4.Runtime;
using CodeBaseContextGenerator.JavaAntlr4.Extrators;
using CodeBaseContextGenerator.JavaAntlr4.Nodes;

namespace CodeBaseContextGenerator.JavaAntlr4.Visitors;

internal sealed class InterfaceVisitor(CommonTokenStream tokenStream) : JavaParserBaseVisitor<InterfaceNode>
{
    public override InterfaceNode VisitInterfaceDeclaration(JavaParser.InterfaceDeclarationContext ctx)
    {
        var modifiers = ModifierExtractor.From(ctx);
        bool isAbstract = modifiers.Contains("abstract");
        string? javadoc = JavadocExtractor.From(ctx, tokenStream);
        
        return new InterfaceNode
        {
            Name = ctx.identifier().GetText(),
            Privacy = modifiers.FirstOrDefault(m => m is "public" or "private" or "protected")
                      ?? "package-private",
            Modifiers = modifiers,
            IsAbstract = isAbstract,
            Javadoc = javadoc,
            Content = ctx.GetText(),
            InheritanceRefs = InheritanceExtractor.From(ctx), // extends interfaces
        };
    }
}