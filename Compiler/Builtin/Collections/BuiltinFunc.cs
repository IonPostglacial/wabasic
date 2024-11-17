using DotNetComp.Compiler.Typing;

namespace DotNetComp.Compiler.Builtin.Collections;

public static class BuiltinFunc
{
    public static readonly TypeDefinition Definition = new(Symbol.BuiltIn("Func"), [new FreeTypeParameter()], BuiltInTypeDef.Object, [], [], [
        new MethodDefinition("Invoke", [], [new AllParentTypeParameters()]),
    ]);
}