namespace DotNetComp.Compiler;

public enum SymKind { Builtin, Local, Member, Constant, Context }

public record struct Symbol(SymKind Kind, string Path, string Name)
{
    private static readonly char[] sigils = ['@', '$', '#', '%'];

    public readonly bool IsGlobalBuiltIn { get => Kind == SymKind.Builtin && Path == ""; }

    public readonly bool Equals(Symbol symbol)
    {
        if (symbol is Symbol sym)
        {
            return 
                Kind == sym.Kind && 
                Path.Equals(sym.Path, StringComparison.OrdinalIgnoreCase) && 
                Name.Equals(sym.Name, StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }

    public override readonly int GetHashCode()
    {
        return HashCode.Combine(
            Kind, 
            StringComparer.OrdinalIgnoreCase.GetHashCode(Path), 
            StringComparer.OrdinalIgnoreCase.GetHashCode(Name)
        );
    }

    private static SymKind SymKindFromSigil(char sigil)
    {
        return sigil switch
        {
            '$' => SymKind.Local,
            '@' => SymKind.Member,
            '#' => SymKind.Constant,
            '%' => SymKind.Context,
            _ => SymKind.Builtin,
        };
    }

    public static Symbol FromText(ReadOnlySpan<char> sym)
    {
        SymKind kind = SymKind.Builtin;
        char sigil = sym[0];
        if (sigils.Contains(sigil))
        {
            kind = SymKindFromSigil(sigil);
            sym = sym[1..];
        }
        Span<Range> pathSym = stackalloc Range[2];
        int pathSymCount = sym.Split(pathSym, '!');
        if (pathSymCount == 2)
            return new Symbol(kind, sym[pathSym[0]].ToString(), sym[pathSym[1]].ToString());
        else
            return new Symbol(kind, "", sym.ToString());
    }

    public static Symbol BuiltIn(string name)
    {
        return new Symbol(SymKind.Builtin, "", name);
    }

    public static Symbol Local(string name)
    {
        return new Symbol(SymKind.Local, "", name);
    }

    public static Symbol Context(string name)
    {
        return new Symbol(SymKind.Context, "", name);
    }
}