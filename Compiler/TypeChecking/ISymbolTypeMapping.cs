using DotNetComp.Compiler.Typing;

namespace DotNetComp.Compiler.TypeChecking;

public interface ISymbolTypeMapping
{
    TypeInstance? Lookup(Symbol symbol);
    ISymbolTypeMapping? Parent { get; }
}