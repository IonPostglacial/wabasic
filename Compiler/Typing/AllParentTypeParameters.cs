namespace DotNetComp.Compiler.Typing;

public record AllParentTypeParameters() : ITypeParameter
{
    public IReadOnlyList<TypeInstance> ResolveConcreteTypes(TypeInstance forType, int offset, int argCount)
    {
        return forType.Arguments;
    }
}
