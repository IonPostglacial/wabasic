namespace DotNetComp.Compiler.Ast;

public enum Accessibility { Public, Private }
public enum ChainingKind { Base, This }
public record ConstructorChaining(ChainingKind Kind, IEnumerable<INode>? Args);

public sealed record ClassDefinition(Range Range, string Name,
    List<MetaAttribute> Attributes,
    List<FunctionParameter>? PrimaryConstructorParams,
    ClassDefinition.Inheritance? Base,
    List<ClassDefinition.Constructor> Constructors,
    List<ClassDefinition.Property> Properties,
    List<ClassDefinition.Method> Methods) : AbstractNode, INode
{
    public T Accept<T>(INodeVisitor<T> visitor)
    {
        return visitor.VisitClassDefinition(this);
    }

    public sealed record Inheritance(Symbol Parent, IEnumerable<INode>? Args);
    public sealed record Getter(List<MetaAttribute> Attributes, Accessibility Accessibility, INode? Body);
    public sealed record Setter(List<MetaAttribute> Attributes, Accessibility Accessibility, FunctionParameter? Param, INode? Body);
    public sealed record Property(List<MetaAttribute> Attributes, Accessibility Accessibility, string Name, TypeName Type, Getter Getter, Setter Setter, INode? Value);
    public sealed record Constructor(List<MetaAttribute> Attributes, Accessibility Accessibility, IEnumerable<FunctionParameter> Params, ConstructorChaining? Chaining, INode Body);
    public sealed record Method(List<MetaAttribute> Attributes, Accessibility Accessibility, string Name, IEnumerable<FunctionParameter> Params, TypeName ReturnType, INode Body)
    {
        public bool Equals(Method? method)
        {
            if (method is not null)
            {
                return 
                    Accessibility == method.Accessibility &&
                    Name.Equals(method.Name, StringComparison.OrdinalIgnoreCase) &&
                    ReturnType == method.ReturnType &&
                    Attributes.SequenceEqual(method.Attributes) &&
                    Params.SequenceEqual(method.Params) &&
                    Convert.ChangeType(method.Body, Body.GetType()).Equals(Body);
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Accessibility, Name, ReturnType, Tools.HashSequence(Attributes), Tools.HashSequence(Params), Body);
        }
    }

    public bool Equals(ClassDefinition? otherClass)
    {
        if (otherClass is not null)
        {
            return 
                Range.Equals(otherClass.Range) && 
                Name == otherClass.Name &&
                Type == otherClass.Type &&
                (
                    (PrimaryConstructorParams is null && otherClass.PrimaryConstructorParams is null) ||
                    (PrimaryConstructorParams is not null && otherClass.PrimaryConstructorParams is not null &&
                        PrimaryConstructorParams.SequenceEqual(otherClass.PrimaryConstructorParams)
                    )
                ) &&
                Methods.SequenceEqual(otherClass.Methods);
        }
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Range, Name, Type, Tools.HashSequence(PrimaryConstructorParams ?? []));
    }
}