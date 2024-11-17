namespace DotNetComp.Compiler.Ast;

public sealed record WhileLoop(Range Range, INode Condition, INode Body) : AbstractNode, ICallableStatement
{
    public static (INode, bool) FromArgList(Range span, List<INode> args)
    {
        return args switch
        {
        [var condition, var body] => (new WhileLoop(span, condition, body), true),
            _ => (new InvalidNode(span), false),
        };
    }

    public int[] Arity() => [2];

    public T Accept<T>(INodeVisitor<T> visitor)
    {
        return visitor.VisitWhileLoop(this);
    }
}
