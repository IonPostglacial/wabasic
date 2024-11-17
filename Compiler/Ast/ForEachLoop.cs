namespace DotNetComp.Compiler.Ast;

public sealed record ForEachLoop(Range Range, INode Iterable, INode Body) : AbstractNode, ICallableStatement
{
    public static (INode, bool) FromArgList(Range span, List<INode> args)
    {
        return args switch
        {
            [var iter, var body] => (new ForEachLoop(span, iter, body), true),
            _ => (new InvalidNode(span), false),
        };
    }

    public int[] Arity() => [2];

    public T Accept<T>(INodeVisitor<T> visitor)
    {
        return visitor.VisitForEachLoop(this);
    }
}
