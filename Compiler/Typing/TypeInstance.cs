using System.Text;
using DotNetComp.Compiler.Ast;
using DotNetComp.Compiler.Builtin;

namespace DotNetComp.Compiler.Typing;

public sealed record TypeInstance(TypeDefinition Definition, List<TypeInstance> Arguments, bool IsNullable = false)
{
    public bool Equals(TypeInstance? typeInstance)
    {
        if (typeInstance is not null)
        {
            return Definition.Equals(typeInstance.Definition) && IsNullable == typeInstance.IsNullable && Arguments.SequenceEqual(typeInstance.Arguments);
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Definition, IsNullable, Tools.HashSequence(Arguments));
    }

    public bool IsCallable()
    {
        return Definition.Name == Symbol.BuiltIn("<callable>");
    }

    public int CallableParamsCount()
    {
        if (Definition.Name == Symbol.BuiltIn("<callable>"))
            return Arguments.Count - 1;
        return 0;
    }

    public TypeInstance? CallableReturnType()
    {
        if (Definition.Name == Symbol.BuiltIn("<callable>"))
            return Arguments.Last();
        return null;
    }
    
    public bool IsSubtype(TypeInstance typeInstance)
    {
        return (!IsNullable || typeInstance.IsNullable) && (
            Definition.Name == typeInstance.Definition.Name || 
            HasForAncestor(typeInstance)
        );
    }

    public bool HasForAncestor(TypeInstance typeInstance)
    {
        if (this == typeInstance)
        {
            return true;
        }
        if (IsNullable && !typeInstance.IsNullable)
        {
            return false;
        }
        if (typeInstance.IsTopType())
        {
            return true;
        }
        TypeInstance ancestor = this with { IsNullable = typeInstance.IsNullable };
        while (!ancestor.IsTopType())
        {
            if (ancestor == typeInstance)
            {
                return true;
            }
            ancestor = ancestor.GetParentType();
        }
        return false;
    }

    private List<TypeInstance> LookupTypes(IEnumerable<ITypeParameter> parameters, int argCount)
    {
        List<TypeInstance> typeArgs = [];

        // TODO: handle variadic type parameters

        foreach (var param in parameters)
        {
            foreach (var t in param.ResolveConcreteTypes(this, 0, argCount))
            {
                typeArgs.Add(t);
            }
        }

        return typeArgs;
    }

    public TypeInstance GetParentType()
    {
        var typeArgs = LookupTypes(Definition.Parameters, Definition.Parent?.Parameters.Count() ?? 0);
        TypeDefinition parentDefinition = Definition.Parent ?? BuiltInTypeDef.Object;
        return parentDefinition.Instanciate(typeArgs, IsNullable);
    }

    public TypeInstance? GetMemberType(string name, int argCount)
    {
        var member = Definition.GetMemberWithName(name);
        if (member is null)
        {
            return null;
        }
        return BuiltInTypeDef.Callable.Instanciate(LookupTypes(member.TypeParameters, argCount));
    }

    public bool IsTopType()
    {
        return Definition.Name.Equals(Symbol.BuiltIn("Object"));
    }

    public TypeInstance LastCommonAncestor(TypeInstance typeInstance)
    {
        TypeInstance self = this;
        if (IsNullable || typeInstance.IsNullable)
        {
            self = this with { IsNullable = true };
            typeInstance = typeInstance with { IsNullable = true };
        }
        while (true)
        {
            if (self.IsSubtype(typeInstance)) 
            {
                return typeInstance;
            }
            typeInstance = typeInstance.GetParentType();
        }
    }

    public override string ToString()
    {
        string nullability = IsNullable ? "" : "+";
        StringBuilder args = new();
        if (Arguments.Count > 0)
        {
            args.Append('<');
            var sep = "";
            foreach (var arg in Arguments)
            {
                args.Append(sep);
                sep = ", ";
                args.Append(arg.ToString());
            }
            args.Append('>');
        }
        return $"{Definition.Name.Name}{nullability}{args}";
    }
}
