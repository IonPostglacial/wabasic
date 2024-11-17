namespace DotNetComp.Compiler.Ast;

static class Tools
{
    public static int HashSequence<T>(IEnumerable<T> sequence)
    {
        int hash = 19;
        foreach (var foo in sequence)
        {
            hash = hash * 31 + foo?.GetHashCode() ?? 0;
        }
        return hash;
    }
}