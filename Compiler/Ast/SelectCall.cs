namespace DotNetComp.Compiler.Ast;

public sealed record SelectCall(Range Range, SelectCall.Kind SelectType, INode Selected, List<INode> Args) : AbstractNode, INode
{
    public enum Kind { Primary, Secondary }

    public bool Equals(SelectCall? select)
    {
        if (select is not null)
        {
            return Range.Equals(select.Range) && Type == select.Type && SelectType == select.SelectType && Args.SequenceEqual(select.Args);
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Range, Type, SelectType, Tools.HashSequence(Args));
    }

    public T Accept<T>(INodeVisitor<T> visitor)
    {
        return visitor.VisitSelect(this);
    }
}
