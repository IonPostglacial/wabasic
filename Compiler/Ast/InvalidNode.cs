namespace DotNetComp.Compiler.Ast;

public sealed record InvalidNode(Range Range) : AbstractNode, INode
{
    public T Accept<T>(INodeVisitor<T> visitor)
    {
        return visitor.VisitInvalidNode(this);
    }
}
