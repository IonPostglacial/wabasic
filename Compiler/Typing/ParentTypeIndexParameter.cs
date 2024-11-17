namespace DotNetComp.Compiler.Typing;

public record ParentTypeIndexParameter(int Index) : ITypeParameter
{
    public IReadOnlyList<TypeInstance> ResolveConcreteTypes(TypeInstance forType, int offset, int argCount)
    {
        return [forType.Arguments[Index + offset]];
    }
}
