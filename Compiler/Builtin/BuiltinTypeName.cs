namespace DotNetComp.Compiler.Builtin;

public static class BuiltinTypeName
{
    public readonly static Ast.TypeName Any = WithName("Any");
    public readonly static Ast.TypeName Object = WithName("Object");
    public readonly static Ast.TypeName Boolean = WithName("Boolean");
    public readonly static Ast.TypeName Number = WithName("Number");
    public readonly static Ast.TypeName String = WithName("String");
    public readonly static Ast.TypeName Date = WithName("Date");

    public static Ast.TypeName WithName(string name)
    {
        return Generic(name, []);
    }

    public static Ast.TypeName Generic(string name, List<Ast.TypeName> args)
    {
        return new(new (SymKind.Builtin, "", name), args, false);
    }
}