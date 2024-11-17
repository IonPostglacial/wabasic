using DotNetComp.Compiler.Typing;

namespace DotNetComp.Compiler.Builtin.Collections;

public static class BuiltinCache
{
    public static readonly TypeDefinition Definition = new(Symbol.BuiltIn("Cache"), 
        [new ParentTypeIndexParameter(0), new ParentTypeIndexParameter(1)], BuiltinDictionary.Definition, [], [], []);

    public static TypeInstance Of(TypeInstance key, TypeInstance value, bool nullable = false)
    {
        return Definition.Instanciate([key, value], nullable);
    }
}