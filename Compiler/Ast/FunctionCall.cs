namespace DotNetComp.Compiler.Ast;

public sealed record FunctionCall(Range Range, INode Callee, List<INode> Args) : AbstractNode, INode
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
        return visitor.VisitFunctionCall(this);
    }
}
