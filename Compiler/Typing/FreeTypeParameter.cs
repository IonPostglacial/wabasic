
namespace DotNetComp.Compiler.Typing;

public class FreeTypeParameter : ITypeParameter
{
    public IReadOnlyList<TypeInstance> ResolveConcreteTypes(TypeInstance forType, int offset, int argCount)
    {
        return [];
    }
}