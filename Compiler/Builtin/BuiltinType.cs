using DotNetComp.Compiler.Typing;

namespace DotNetComp.Compiler.Builtin;

public static class BuiltinType
{
    public static readonly TypeInstance Object = BuiltInTypeDef.Object.Instanciate(); 
    public static readonly TypeInstance Boolean = BuiltInTypeDef.Boolean.Instanciate(); 
    public static readonly TypeInstance Number = BuiltInTypeDef.Number.Instanciate(); 
    public static readonly TypeInstance String = BuiltInTypeDef.String.Instanciate(); 
    public static readonly TypeInstance Date = BuiltInTypeDef.Date.Instanciate(); 
    public static readonly TypeInstance Buffer = BuiltInTypeDef.Buffer.Instanciate(); 
}