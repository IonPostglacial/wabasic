
namespace DotNetComp.Compiler.Typing;

public class VarargTypeParameter(ITypeParameter TypeParameter) : ITypeParameter
{
    public IReadOnlyList<TypeInstance> ResolveConcreteTypes(TypeInstance forType, int offset, int argCount)
    {
        int missingArgsCount = argCount - offset;
        List<TypeInstance> concreteTypes = [];
        while (concreteTypes.Count < missingArgsCount)
        {
            foreach (var type in TypeParameter.ResolveConcreteTypes(forType, offset, argCount))
            {
                concreteTypes.Add(type);
            }
        }
        return concreteTypes;
    }
} 