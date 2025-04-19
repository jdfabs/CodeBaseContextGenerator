using Antlr4.Runtime;
using CodeBaseContextGenerator.JavaAntlr4.Builders;
using CodeBaseContextGenerator.JavaAntlr4.Visitors;

namespace CodeBaseContextGenerator.JavaAntlr4.Base;

public sealed class JavaClassInspector
{
    private readonly string _rootPath;
    private readonly string _sourcePath;

    public JavaClassInspector(string fullSourcePath, string rootPath)
    {
        _sourcePath = fullSourcePath;
        _rootPath   = rootPath;
    }

    public IReadOnlyList<TypeRepresentation> Inspect(string code)
    {
        var input  = new AntlrInputStream(code);
        var lexer  = new JavaLexer(input);
        var tokens = new CommonTokenStream(lexer);
        var parser = new JavaParser(tokens) { BuildParseTree = true };

        var tree = parser.compilationUnit();

        // 1. Walk once, collect raw facts
        var visitor = new CompilationUnitVisitor(_rootPath, _sourcePath);
        visitor.Visit(tree);

        // 2. Build DTOs from the facts
        return new TypeRepresentationBuilder(_rootPath, _sourcePath)
            .BuildAll(visitor.CollectedNodes);
    }
}
