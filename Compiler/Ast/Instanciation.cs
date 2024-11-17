namespace DotNetComp.Compiler.Ast;

public sealed record Instanciation(Range Range, TypeName Constructor, List<INode> Args) : AbstractNode, INode
{
    public bool Equals(Instanciation? node)
    {
        if (node is not null)
        {
            return Range.Equals(node.Range) && Type == node.Type && Constructor == node.Constructor && Args.SequenceEqual(node.Args);
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Range, Type, Constructor, Tools.HashSequence(Args));
    }

    public T Accept<T>(INodeVisitor<T> visitor)
    {
        return visitor.VisitInstanciation(this);
    }
}
