namespace CodeBaseContextGenerator.Core.Interfaces.Extractors.Relationship;

public interface IImportExtractor<in TNode> : IExtractor<TNode,IReadOnlyCollection<string>> {
}
