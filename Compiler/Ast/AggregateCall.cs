namespace DotNetComp.Compiler.Ast;

public sealed record AggregateCall(Range Range, INode Aggregated, Symbol Aggregation, INode Filter) : AbstractNode, INode
{
    public T Accept<T>(INodeVisitor<T> visitor)
    {
       return visitor.VisitAggregateCall(this);
    }
}
