namespace CodeBaseContextGenerator.Core.Interfaces.Extractors.Relationship;

public interface IImportExtractor<in TNode>
{
    protected IReadOnlyCollection<string> ExtractImports(TNode node);
}