namespace DotNetComp.Compiler.Ast;

public sealed record VariableDeclaration(Range Range, TypeName? DeclaredType, Symbol Symbol, INode? Value) : AbstractNode, INode
{
    public T Accept<T>(INodeVisitor<T> visitor)
    {
        return visitor.VisitVariableDeclaration(this);
    }
}
