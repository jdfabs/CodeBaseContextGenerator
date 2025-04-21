using CodeBaseContextGenerator.Core.Interfaces.Representations;

namespace CodeBaseContextGenerator.Core.Interfaces.Visitors;

public interface IMethodVisitor<TNode> : IAstVisitor<TNode, IMethodRepresentation>
{
}

public interface IConstructorVisitor<TNode> : IAstVisitor<TNode, IConstructorRepresentation>
{
}

public interface IFieldVisitor<TNode> : IAstVisitor<TNode, IReadOnlyCollection<IFieldRepresentation>>
{
}