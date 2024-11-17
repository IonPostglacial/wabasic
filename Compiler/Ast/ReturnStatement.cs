namespace DotNetComp.Compiler.Ast;

public sealed record ReturnStatement(Range Range, INode Value) : AbstractNode, ICallableStatement
{
    public static (INode, bool) FromArgList(Range span, List<INode> args)
    {
        if (args.Count == 1)
        {
            return (new ReturnStatement(span, args[0]), true);
        }
        return (new InvalidNode(span), false);
    }

    public int[] Arity() => [0, 1];

    public T Accept<T>(INodeVisitor<T> visitor)
    {
        return visitor.VisitReturnStatement(this);
    }
}
