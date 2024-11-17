namespace DotNetComp.Compiler.Ast;

public sealed record MetaAttribute(Symbol Name, List<INode> Arguments)
{
    public bool Equals(MetaAttribute? attribute)
    {
        if (attribute is not null)
        {
            return Name.Equals(attribute.Name) && Arguments.SequenceEqual(attribute.Arguments);
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Tools.HashSequence(Arguments));
    }
}