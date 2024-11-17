namespace DotNetComp.Compiler.Ast;

public sealed record IfStatement(Range Range, INode Condition, INode Then, INode? Else) : AbstractNode, ICallableStatement
{
    public static (INode, bool) FromArgList(Range span, List<INode> args)
    {
        return args switch
        {
            [var condition, var then, var otherwise] => (new IfStatement(span, condition, then, otherwise), true),
            [var condition, var then] => (new IfStatement(span, condition, then, null), true),
            _ => (new InvalidNode(span), false),
        };
    }

    public int[] Arity() => [2, 3];

    public T Accept<T>(INodeVisitor<T> visitor)
    {
        return visitor.VisitIfStatement(this);
    }
}
