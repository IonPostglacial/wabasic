namespace DotNetComp.Compiler.Ast;

public sealed record Lambda(Range Range, List<FunctionParameter> Params, INode Body) : AbstractNode, INode
{
    public bool Equals(Lambda? lambda)
    {
        if (lambda is not null)
        {
            return 
                Range.Equals(lambda.Range) && 
                Type == lambda.Type && Params.SequenceEqual(lambda.Params) && 
                Convert.ChangeType(lambda.Body, Body.GetType()).Equals(Body);
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Range, Type, Tools.HashSequence(Params), Body);
    }

    public T Accept<T>(INodeVisitor<T> visitor)
    {
        return visitor.VisitLambda(this);
    }
}
