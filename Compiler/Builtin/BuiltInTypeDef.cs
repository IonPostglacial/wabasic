using DotNetComp.Compiler.Typing;

namespace DotNetComp.Compiler.Builtin;

public static class BuiltInTypeDef
{
    public static readonly TypeDefinition Object = new(Symbol.BuiltIn("Object"), [], null, [], [], []);
    public static readonly TypeDefinition Callable = new(Symbol.BuiltIn("<callable>"), [], Object, [], [], []); 
    public static readonly TypeDefinition Boolean = new(Symbol.BuiltIn("Boolean"), [], Object, [], [], []); 
    public static readonly TypeDefinition Number = new(Symbol.BuiltIn("Number"), [], Object, [], [], []); 
    public static readonly TypeDefinition String = new(Symbol.BuiltIn("String"), [], Object, [], [], []); 
    public static readonly TypeDefinition Date = new(Symbol.BuiltIn("Date"), [], Object, [], [], []); 
    public static readonly TypeDefinition Buffer = new(Symbol.BuiltIn("Buffer"), [], Object, [], [], []);
    public static readonly TypeDefinition KeyValuePair = new(Symbol.BuiltIn("KeyValuePair"), [new FreeTypeParameter(), new FreeTypeParameter()], Object, [], [], []);
}