using DotNetComp.Compiler.Typing;

namespace DotNetComp.Compiler.Ast;

public abstract record AbstractNode
{
    public TypeInstance? Type { get; set; }
}