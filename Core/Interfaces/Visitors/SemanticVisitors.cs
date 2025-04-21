using CodeBaseContextGenerator.Core.Interfaces.Representations;

namespace CodeBaseContextGenerator.Core.Interfaces.Visitors;

public interface ITypeUsageVisitor<TNode> : IAstVisitor<TNode, IReadOnlyCollection<ITypeReference>>
{
}

public interface IAttributeVisitor<TNode> : IAstVisitor<TNode, IReadOnlyCollection<string>>
{
}

public interface IDocCommentVisitor<TNode> : IAstVisitor<TNode, string?>
{
}