using CodeBaseContextGenerator.Core.Interfaces.Representations;

namespace CodeBaseContextGenerator.Core.Interfaces.Assemblers;

public interface IFileContextAssembler<TRoot> : IAssembler<TRoot, IReadOnlyCollection<ITypeRepresentation>> { }