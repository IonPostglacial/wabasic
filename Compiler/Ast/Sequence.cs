namespace DotNetComp.Compiler.Ast;

public sealed record Sequence(Range Range, List<INode> Expressions) : AbstractNode, INode
{
    public bool Equals(Sequence? seq)
    {
        if (seq is not null)
        {
            return Range.Equals(seq.Range) && Type == seq.Type && Expressions.SequenceEqual(seq.Expressions);
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Range, Type, Tools.HashSequence(Expressions));
    }

    public T Accept<T>(INodeVisitor<T> visitor)
    {
        return visitor.VisitSequence(this);
    }
}
