using DotNetComp.Compiler.Typing;

namespace DotNetComp.Compiler.Builtin.Collections;

public static class BuiltinDictionary
{
    public static readonly TypeDefinition Definition = new(Symbol.BuiltIn("Dictionary"), [new FreeTypeParameter()], BuiltInTypeDef.Object, [], [], [
        new MethodDefinition("New", [], [new ThisTypeParameters()]),
        new MethodDefinition("Count", [], [new FixedTypeParameter(BuiltinType.Number)]),
        new MethodDefinition("Get", [], [new ParentTypeIndexParameter(0), new ParentTypeIndexParameter(1)]),
        new MethodDefinition("Set", [], [new ParentTypeIndexParameter(0), new ParentTypeIndexParameter(1), new FixedTypeParameter(BuiltinType.Object)]),
    ]);

    public static TypeInstance Of(TypeInstance key, TypeInstance value, bool nullable = false)
    {
        return Definition.Instanciate([key, value], nullable);
    }
}