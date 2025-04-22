namespace CodeBaseContextGenerator.Core.Interfaces.Extractors.Relationship;

public interface ISourceAnchorExtractor<in TNode>
{
    protected (string FilePath, int? StartLine, int? EndLine, int? StartColumn, int? EndColumn)
        ExtractSourceAnchor(TNode node);
}