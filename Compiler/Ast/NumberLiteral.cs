namespace DotNetComp.Compiler.Ast;

public sealed record NumberLiteral(Range Range, double Value) : AbstractNode, INode
{
    public T Accept<T>(INodeVisitor<T> visitor)
    {
        return visitor.VisitNumberLiteral(this);
    }
}
