namespace DotNetComp.Compiler.Ast;

public sealed record BinaryOperation(BinaryOperator Operator, INode Left, INode Right) : AbstractNode, INode
{
    public Range Range => new(Left.Range.Start, Right.Range.End);

    public T Accept<T>(INodeVisitor<T> visitor)
    {
        return visitor.VisitBinaryOperation(this);
    }
}
