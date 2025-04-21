using CodeBaseContextGenerator.Core.Interfaces.Representations;

namespace CodeBaseContextGenerator.Core.Interfaces.Visitors;

public interface IClassVisitor<TNode> : IAstVisitor<TNode, ITypeRepresentation>
{
}

public interface IInterfaceVisitor<TNode> : IAstVisitor<TNode, ITypeRepresentation>
{
}

public interface IEnumVisitor<TNode> : IAstVisitor<TNode, ITypeRepresentation>
{
}

public interface IRecordVisitor<TNode> : IAstVisitor<TNode, ITypeRepresentation>
{
}

public interface INestedTypeVisitor<TNode> : IAstVisitor<TNode, IReadOnlyCollection<ITypeRepresentation>>
{
}