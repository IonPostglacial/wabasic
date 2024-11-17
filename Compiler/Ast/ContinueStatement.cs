namespace DotNetComp.Compiler.Ast;

public sealed record ContinueStatement(Range Range) : AbstractNode, ICallableStatement
{
    public static (INode, bool) FromArgList(Range span, List<INode> args)
    {
        if (args.Count == 0)
        {
            return (new ContinueStatement(span), true);
        }
        return (new InvalidNode(span), false);
    }

    public int[] Arity() => [0, 1];

    public T Accept<T>(INodeVisitor<T> visitor)
    {
        return visitor.VisitContinueStatement(this);
    }
}
