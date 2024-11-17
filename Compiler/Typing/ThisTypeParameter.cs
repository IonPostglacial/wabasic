namespace DotNetComp.Compiler.Typing;

public record ThisTypeParameters() : ITypeParameter
{
    public IReadOnlyList<TypeInstance> ResolveConcreteTypes(TypeInstance forType, int offset, int argCount)
    {
        return [forType];
    }
}