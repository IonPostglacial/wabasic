namespace DotNetComp.Compiler.Typing;

public record ParentTypeRangeParameter(Range Range) : ITypeParameter
{
    public IReadOnlyList<TypeInstance> ResolveConcreteTypes(TypeInstance forType, int offset, int argCount)
    {
        return forType.Arguments[(Range.Start.Value + offset)..(Range.End.Value + offset)];
    }
}
