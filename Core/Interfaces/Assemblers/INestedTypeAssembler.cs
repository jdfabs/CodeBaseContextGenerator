using CodeBaseContextGenerator.Core.Interfaces.Representations;

namespace CodeBaseContextGenerator.Core.Interfaces.Assemblers;

public interface INestedTypeAssembler<TNode> : IAssembler<TNode, IReadOnlyCollection<ITypeRepresentation>> { }