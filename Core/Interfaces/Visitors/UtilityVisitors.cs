namespace CodeBaseContextGenerator.Core.Interfaces.Visitors;

public interface IContextSummaryVisitor<TNode> : IAstVisitor<TNode, string>
{
}