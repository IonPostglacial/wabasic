namespace DotNetComp.Compiler.Ast;

public sealed record MethodCall(Range Range, INode This, Symbol Method, List<INode> Args) : AbstractNode, INode
{
    public bool Equals(FunctionCall? call)
    {
        if (call is not null)
        {
            return Range.Equals(call.Range) && Type == call.Type && Args.SequenceEqual(call.Args);
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Range, Type, Tools.HashSequence(Args));
    }

    public T Accept<T>(INodeVisitor<T> visitor)
    {
        return visitor.VisitMethodCall(this);
    }
}
