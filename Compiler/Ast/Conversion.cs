namespace DotNetComp.Compiler.Ast;

public sealed record Conversion(Range Range, Conversion.Operation Kind, INode Node, TypeName ToType) : AbstractNode, INode
{
    public T Accept<T>(INodeVisitor<T> visitor)
    {
        return visitor.VisitConversion(this);
    }

    public enum Operation { Cast, TryCast, DeserializeFromJson }
}
