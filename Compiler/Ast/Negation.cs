namespace DotNetComp.Compiler.Ast;

public sealed record Negation(Range Range, INode Child) : AbstractNode, INode
{
    public T Accept<T>(INodeVisitor<T> visitor)
    {
        return visitor.VisitNegation(this);
    }
}
