namespace DotNetComp.Compiler.Ast;

public sealed record TypeName(Symbol Name, List<TypeName> Args, bool IsNullable)
{
    public bool Equals(TypeName? typeName)
    {
        if (typeName is not null)
        {
            return Name == typeName.Name && IsNullable == typeName.IsNullable &&  Args.SequenceEqual(typeName.Args);
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, IsNullable, Tools.HashSequence(Args));
    }
}