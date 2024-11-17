namespace DotNetComp.Compiler.Ast;

public sealed record StringLiteral(Range Range, string Value) : AbstractNode, INode
{
    public T Accept<T>(INodeVisitor<T> visitor)
    {
        return visitor.VisitStringLiteral(this);
    }
}
