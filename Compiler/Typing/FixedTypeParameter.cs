namespace DotNetComp.Compiler.Typing;

public record FixedTypeParameter(TypeInstance TypeInstance) : ITypeParameter
{
    public IReadOnlyList<TypeInstance> ResolveConcreteTypes(TypeInstance forType, int offset, int argCount)
    {
        return [TypeInstance];
    }
}
