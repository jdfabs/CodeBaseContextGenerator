using Antlr4.Runtime;
using CodeBaseContextGenerator.Core.Interfaces.Extractors.Relationship;

namespace CodeBaseContextGenerator.Languages.JavaAntlr4.Extrators__;

public class JavaSourceAnchorExtractor : ISourceAnchorExtractor<ParserRuleContext>
{
    public (string FilePath, int? StartLine, int? EndLine, int? StartColumn, int? EndColumn) Extract(ParserRuleContext node)
    {
        if (node.Start == null || node.Stop == null)
            return (string.Empty, null, null, null, null);

        var startLine = node.Start.Line;
        var endLine = node.Stop.Line;
        var startColumn = node.Start.Column;
        var endColumn = node.Stop.Column;

        var filePath = new JavaFullQualifiedNameExtractor().Extract(node);
        return (filePath, startLine, endLine, startColumn, endColumn);
    }
}