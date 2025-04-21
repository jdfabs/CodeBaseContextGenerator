namespace CodeBaseContextGenerator.Core.Interfaces.Extractors.Relationship;

public interface ISourceAnchorExtractor<in TNode> : IExtractor<TNode,(string FilePath, int? StartLine, int? EndLine, int? StartColumn, int? EndColumn) > {
}
