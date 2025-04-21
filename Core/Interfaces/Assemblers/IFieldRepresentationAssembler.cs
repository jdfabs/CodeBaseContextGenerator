using CodeBaseContextGenerator.Core.Interfaces.Representations;

namespace CodeBaseContextGenerator.Core.Interfaces.Assemblers;

public interface IFieldRepresentationAssembler<TNode> : IAssembler<TNode, IReadOnlyCollection<IFieldRepresentation>> { }
