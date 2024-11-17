namespace DotNetComp.Compiler.Typing;

public interface ITypeParameter
{
    IReadOnlyList<TypeInstance> ResolveConcreteTypes(TypeInstance forType, int offset, int argCount);
}
