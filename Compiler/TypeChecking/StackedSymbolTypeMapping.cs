using DotNetComp.Compiler.Typing;

namespace DotNetComp.Compiler.TypeChecking;

class StackedSymbolTypeMapping(ISymbolTypeMapping? Parent) : ISymbolTypeMapping
{
    public ISymbolTypeMapping? Parent { get; init; } = Parent;
    private readonly Dictionary<Symbol, TypeInstance> typeBySymbol = [];

    public TypeInstance? Lookup(Symbol symbol)
    {
        typeBySymbol.TryGetValue(symbol, out var typeInstance);
        if (typeInstance is null)
            return Parent?.Lookup(symbol);
        return typeInstance;
    }

    public void BindSymbol(Symbol symbol, TypeInstance instance)
    {
        typeBySymbol.Add(symbol, instance);
    }
}