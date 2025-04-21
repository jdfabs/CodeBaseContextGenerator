namespace CodeBaseContextGenerator.Core.Interfaces.Assemblers;

public interface IAssembler<in TNode, out TResult>
{
     TResult Assemble(TNode node);
}