namespace DotNetComp.Compiler.Ast;

public sealed record Variable(Range Range, Symbol Symbol) : AbstractNode, INode
{
    public T Accept<T>(INodeVisitor<T> visitor)
    {
        return visitor.VisitVariable(this);
    }
}
