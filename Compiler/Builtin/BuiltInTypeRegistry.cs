using DotNetComp.Compiler.Ast;
using DotNetComp.Compiler.Builtin;
using DotNetComp.Compiler.Builtin.Collections;
using DotNetComp.Compiler.Typing;

namespace DotNetComp.Compiler.TypeChecking;

public class BuiltInTypeRegistry : ITypeRegistry
{
    private readonly Dictionary<Symbol, TypeDefinition> typeDefsByName = [];

    public void Register(TypeDefinition definition)
    {
        typeDefsByName.Add(definition.Name, definition);
    }

    public BuiltInTypeRegistry()
    {
        typeDefsByName.Add(Symbol.BuiltIn("Any"), BuiltInTypeDef.Object);
        Register(BuiltInTypeDef.Object);
        Register(BuiltInTypeDef.Boolean);
        Register(BuiltInTypeDef.Number);
        Register(BuiltInTypeDef.String);
        Register(BuiltInTypeDef.Date);
        Register(BuiltinArray.Definition);
        Register(BuiltinDictionary.Definition);
        Register(BuiltinFunc.Definition);
    }

    public TypeDefinition? Lookup(Symbol name)
    {
        return typeDefsByName.GetValueOrDefault(name);
    }

    public TypeInstance? Lookup(TypeName typeName)
    {
        typeDefsByName.TryGetValue(typeName.Name, out var typeDef);
        if (typeDef is not null)
        {
            List<TypeInstance> args = [];
            foreach (var arg in typeName.Args)
            {
                var definition = Lookup(arg);
                if (definition is null)
                {
                    return null;
                }
                args.Add(definition);
            }
            return typeDef.Instanciate(args, typeName.IsNullable);
        }
        return null;
    }
}