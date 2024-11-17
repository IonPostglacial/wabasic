namespace DotNetComp.Compiler.Ast;

public sealed record ListLiteral(Range Range, List<INode> Elements) : AbstractNode, INode
{
    public bool Equals(ListLiteral? list)
    {
        if (list is not null)
        {
            return Range.Equals(list.Range) && Type == list.Type && Elements.SequenceEqual(list.Elements);
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Range, Type, Tools.HashSequence(Elements));
    }

    public T Accept<T>(INodeVisitor<T> visitor)
    {
        return visitor.VisitListLiteral(this);
    }
}
