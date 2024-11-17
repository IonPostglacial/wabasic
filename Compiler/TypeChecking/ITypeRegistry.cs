using DotNetComp.Compiler.Ast;
using DotNetComp.Compiler.Typing;

namespace DotNetComp.Compiler.TypeChecking;

public interface ITypeRegistry
{
    void Register(TypeDefinition definition);
    public TypeDefinition? Lookup(Symbol name);
    public TypeInstance? Lookup(TypeName typeName);
}