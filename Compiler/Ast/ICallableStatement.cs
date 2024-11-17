namespace DotNetComp.Compiler.Ast;

public interface ICallableStatement : INode
{
    public int[] Arity();
}