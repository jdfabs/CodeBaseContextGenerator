namespace CodeBaseContextGenerator.Core.Interfaces.Visitors;

public interface IAstVisitor<in TNode, out TResult>
{
    TResult Visit(TNode node);
}