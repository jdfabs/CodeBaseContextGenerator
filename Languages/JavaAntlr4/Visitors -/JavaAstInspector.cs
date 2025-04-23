using Antlr4.Runtime;
using CodeBaseContextGenerator.Core.Models.Representations;
using CodeBaseContextGenerator.JavaAntlr4.Builders;

namespace CodeBaseContextGenerator.JavaAntlr4.Visitors;

/// <summary>
/// Facade that glues the ANTLR parse phase (<see cref="CompilationUnitVisitor"/>)
/// to the DTO phase (<see cref="TypeRepresentationBuilder"/>).
/// </summary>
public sealed class JavaAstInspector
{
    private readonly string _rootPath;

    public JavaAstInspector(string rootPath)
        => _rootPath = rootPath;

    /// <summary>Inspect a single <c>.java</c> source file.</summary>
    public IReadOnlyCollection<TypeRepresentationBase> Inspect(string sourcePath)
    {
        var code   = File.ReadAllText(sourcePath);
        var input  = new AntlrInputStream(code);
        var lexer  = new JavaLexer(input);
        var tokens = new CommonTokenStream(lexer);
        var parser = new JavaParser(tokens) { BuildParseTree = true };

        // 1️⃣ Collect raw AST nodes (classes/interfaces/methods)
        var cuVisitor = new CompilationUnitVisitor(_rootPath, sourcePath, tokens);
        cuVisitor.Visit(parser.compilationUnit());

        // 2️⃣ Convert to rich TypeRepresentation objects
        var builder = new TypeRepresentationBuilder(_rootPath, sourcePath);
        return builder.BuildAll(cuVisitor.CollectedNodes);
    }
}