using Antlr4.Runtime;
using CodeBaseContextGenerator.Core.Models.Representations;
using CodeBaseContextGenerator.JavaAntlr4.Builders;
using CodeBaseContextGenerator.Languages.JavaAntlr4.Visitors__;

namespace CodeBaseContextGenerator.JavaAntlr4.Visitors;

/// <summary>
/// Top‑level visitor that walks a Java parse tree and collects a flat list
/// </summary>
public sealed class CompilationUnitVisitor : JavaParserBaseVisitor<object?>
{
    private readonly CommonTokenStream _tokenStream;
    

    public CompilationUnitVisitor(string rootPath, string sourcePath, CommonTokenStream tokenStream)
    {
        _tokenStream = tokenStream;
    }

    /// <summary>All types gathered from the compilation unit.</summary>
    public IReadOnlyCollection<TypeRepresentationBase> CollectedNodes => newNodes;
    
    
    public List<TypeRepresentationBase> newNodes = new();

    /*────────────────────────  Dispatch  ─────────────────────────*/

    public override object? VisitTypeDeclaration(JavaParser.TypeDeclarationContext ctx)
    {
        if (ctx.classDeclaration() is { } cls)
        {
            var assembler = new JavaRepresentationAssembler(_tokenStream);
            newNodes.Add(assembler.CreateRepresentation(cls, _tokenStream));
        }
        else if (ctx.interfaceDeclaration() is { } iface)
        {
            var assembler = new JavaRepresentationAssembler(_tokenStream);
            newNodes.Add(assembler.CreateRepresentation(iface, _tokenStream));
        }
        else if (ctx.enumDeclaration() is { } enums)
        {
            var assembler = new JavaRepresentationAssembler(_tokenStream);
            newNodes.Add(assembler.CreateRepresentation(enums, _tokenStream));
        }
        else if (ctx.recordDeclaration() is {} record)
        {
            var assembler = new JavaRepresentationAssembler(_tokenStream);
            newNodes.Add(assembler.CreateRepresentation(record, _tokenStream));
        }
        else
        {
            throw new NotSupportedException($"Unsupported type declaration: {ctx.GetText()}");
        }
        
        return null; // prevent default deep‑walk – visitors handle children
    }
}
