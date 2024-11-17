namespace DotNetComp.Compiler.Ast;

public sealed record ForLoop(Range Range, INode Number, INode Body) : AbstractNode, ICallableStatement
{
    public static (INode, bool) FromArgList(Range span, List<INode> args)
    {
        return args switch
        {
        [var number, var body] => (new ForLoop(span, number, body), true),
            _ => (new InvalidNode(span), false),
        };
    }

    public int[] Arity() => [2];

    public T Accept<T>(INodeVisitor<T> visitor)
    {
        return visitor.VisitForLoop(this);
    }
}
