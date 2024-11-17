using DotNetComp.Compiler.Typing;

namespace DotNetComp.Compiler.Builtin.Collections;

public static class BuiltinArray
{
    public static readonly TypeDefinition Definition = new(Symbol.BuiltIn("Array"), [new FreeTypeParameter()], BuiltInTypeDef.Object, [], [], [
        new MethodDefinition("New", [], [
            new VarargTypeParameter(new ParentTypeIndexParameter(0)), 
            new ThisTypeParameters()
        ]),
        new MethodDefinition("Size", [], [new FixedTypeParameter(BuiltinType.Number)]),
        new MethodDefinition("Get", [], [new FixedTypeParameter(BuiltinType.Number), new ParentTypeIndexParameter(0)]),
        new MethodDefinition("Set", [], [new FixedTypeParameter(BuiltinType.Number), new ParentTypeIndexParameter(0), new FixedTypeParameter(BuiltinType.Object)]),
        new MethodDefinition("Append", [], [new ParentTypeIndexParameter(0), new FixedTypeParameter(BuiltinType.Object)]),
        new MethodDefinition("Remove", [], [new FixedTypeParameter(BuiltinType.Number), new FixedTypeParameter(BuiltinType.Object)]),
        new MethodDefinition("Redim", [], [new FixedTypeParameter(BuiltinType.Number), new FixedTypeParameter(BuiltinType.Object)]),
    ]);

    public static TypeInstance Of(TypeInstance typeInstance)
    {
        return Definition.Instanciate([typeInstance]);
    }
}