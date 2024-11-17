using DotNetComp.Compiler.Typing;

namespace DotNetComp.Compiler.Ast;

public interface INode
{
    public Range Range { get; }
    public TypeInstance? Type { get; set; }
    public T Accept<T>(INodeVisitor<T> visitor);
}