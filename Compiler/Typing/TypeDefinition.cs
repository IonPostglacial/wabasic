using DotNetComp.Compiler.Ast;
using DotNetComp.Compiler.Builtin;

namespace DotNetComp.Compiler.Typing;

public class TypeDefinition
{
    public Symbol Name { get; init; }
    public IEnumerable<MetaAttribute> Attributes { get; init; }
    public IEnumerable<ITypeParameter> Parameters { get; init; }
    public TypeDefinition? Parent { get; init; }
    public IEnumerable<PropertyDefinition> Properties { get; init; }
    public IEnumerable<MethodDefinition> Methods { get; init; }
    private readonly TypeDefinition[] hierarchy = [];

    public override bool Equals(object? obj)
    {
        if (obj is TypeDefinition typeDefinition)
        {
            return Name.Equals(typeDefinition.Name);
        }
        return false;
    }
    
    public override int GetHashCode()
    {
        return Name.GetHashCode();
    }

    public TypeDefinition(
        Symbol name,
        IEnumerable<ITypeParameter> parameters,
        TypeDefinition? parent,
        IEnumerable<MetaAttribute> attributes,
        IEnumerable<PropertyDefinition> properties,
        IEnumerable<MethodDefinition> methods)
    {
        Name = name;
        Parameters = parameters;
        Parent = parent;
        Attributes = attributes;
        Properties = properties;
        Methods = methods;
        if (parent is not null)
        {
            hierarchy = new TypeDefinition[parent.hierarchy.Length + 1];
            parent.hierarchy.AsSpan().CopyTo(hierarchy);
            hierarchy[^1] = parent;
        }
    }

    public bool HasForAncestor(TypeDefinition typeDefinition)
    {
        return hierarchy.Contains(typeDefinition);
    }

    public TypeDefinition LastCommonAncestor(TypeDefinition typeDefinition)
    {
        if (this == typeDefinition || this.HasForAncestor(typeDefinition))
        {
            return typeDefinition;
        }
        foreach (var parent in hierarchy)
        {
            if (parent.HasForAncestor(this))
            {
                return parent;
            }
        }
        return BuiltInTypeDef.Object;
    }

    public MethodDefinition? GetMemberWithName(string name)
    {
        foreach (var member in Methods)
        {
            if (member.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
            {
                return member;
            }
        }
        return null;
    }

    public TypeInstance Instanciate(bool IsNullable = false)
    {
        return new TypeInstance(this, [], IsNullable);
    }

    public TypeInstance Instanciate(List<TypeInstance> arguments, bool IsNullable = false)
    {
        return new TypeInstance(this, arguments, IsNullable);
    }
}