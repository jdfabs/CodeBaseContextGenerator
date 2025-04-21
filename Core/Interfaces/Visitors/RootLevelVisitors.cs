using CodeBaseContextGenerator.Core.Interfaces.Representations;

namespace CodeBaseContextGenerator.Core.Interfaces.Visitors;

public interface ICompilationUnitVisitor<TNode> : IAstVisitor<TNode, IReadOnlyCollection<ITypeRepresentation>>
{
}

public interface IImportVisitor<TNode> : IAstVisitor<TNode, IReadOnlyCollection<string>>
{
}

public interface IRootContextVisitor<TRoot> : IAstVisitor<TRoot, IReadOnlyCollection<ITypeRepresentation>>
{
}