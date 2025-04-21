using Antlr4.Runtime;
using CodeBaseContextGenerator.JavaAntlr4.Builders;
using CodeBaseContextGenerator.JavaAntlr4.Extrators;
using CodeBaseContextGenerator.JavaAntlr4.Nodes;

namespace CodeBaseContextGenerator.JavaAntlr4.Visitors;

internal sealed class ClassVisitor(string rootPath, string sourcePath, CommonTokenStream tokenStream)
    : JavaParserBaseVisitor<ClassNode>
{
    public override ClassNode VisitClassDeclaration(JavaParser.ClassDeclarationContext ctx)
    {
        var methods = ctx.classBody()?.classBodyDeclaration()
            .Where(b => b.memberDeclaration()?.methodDeclaration() != null)
            .Select(b => b.memberDeclaration().methodDeclaration())
            .Select(md => MethodVisitor.Build(md, rootPath, sourcePath , tokenStream))
            .ToArray() ?? System.Array.Empty<AstMethodNode>();

// 1) Modifiers & abstract
        var modifiers = ModifierExtractor.From(ctx);
        //bool isAbstract = modifiers.Contains("abstract");

// 2) Javadoc (via a helper that looks for ctx.parent().getChild(0) if it's a comment)
        string? javadoc = JavadocExtractor.From(ctx, tokenStream);

// 3) Fields
        var fields = ctx.classBody()
            .classBodyDeclaration()
            .Where(d => d.memberDeclaration()?.fieldDeclaration() != null)
            .Select(d => FieldVisitor.Build(d.memberDeclaration().fieldDeclaration(), tokenStream))
            .ToArray();

// 4) Constructors
        var ctors = ctx.classBody()
            .classBodyDeclaration()
            .Where(d => d.memberDeclaration()?.constructorDeclaration() != null)
            .Select(d => ConstructorVisitor.Build(d.memberDeclaration().constructorDeclaration(), tokenStream))
            .ToArray();

    

            
        return new ClassNode
        {
            Name = ctx.identifier().GetText(),
            Modifiers = modifiers,
            //IsAbstract = isAbstract,
            Content = ctx.GetText(),
            Javadoc = javadoc,
            Fields = fields,
            Constructors = ctors,
            InheritanceRefs = InheritanceExtractor.From(ctx),
            Methods = methods,
        };
    }
}