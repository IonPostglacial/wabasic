namespace DotNetComp.Compiler.Ast;

public sealed record UnaryMinus(Range Range, INode Child) : AbstractNode, INode
{
    public T Accept<T>(INodeVisitor<T> visitor)
    {
        return visitor.VisitUnaryMinus(this);
    }
}
