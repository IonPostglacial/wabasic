
namespace DotNetComp.Compiler.JsGeneration;

public record JavaScriptClassConstructor(IEnumerable<string> Params, IJavaScriptStatement Body);
public record JavaScriptClassProperty(string Name, IJavaScriptStatement? InitialValue);
public record JavaScriptMethod(string Name, IEnumerable<string> Params, IJavaScriptStatement Body);

public record JavaScriptClass(
    string Name, string? Extends,
    JavaScriptClassConstructor Constructor,
    IEnumerable<JavaScriptClassProperty> Properties,
    IEnumerable<JavaScriptMethod> Methods) : IJavaScriptStatement
{
    public IJavaScriptStatement AsExpression() => this;

    public bool IsExpression() => true;

    public string ToCode()
    {
        string extends = "";
        if (Extends is not null)
        {
            string superName = JavaScriptVariableExpression.NormalizeName(Extends);
            extends = $" extends {superName}";
        }
        string constructorParams = string.Join(", ", Constructor.Params.Select(JavaScriptVariableExpression.NormalizeName));
        string constructorBody = Constructor.Body.ToCode();
        IEnumerable<string> properties = Properties.Select(PropertyToCode);
        string propertiesCode = string.Join(" ", properties);
        IEnumerable<string> methods = Methods.Select(MethodToCode);
        string methodsCode = string.Join(" ", methods);

        return $"class {Name}{extends} {{ {propertiesCode} constructor({constructorParams}) {{ {constructorBody} }} {methodsCode} }}";
    }

    private static string PropertyToCode(JavaScriptClassProperty property)
    {
        string propertyName = JavaScriptVariableExpression.NormalizeName(property.Name);
        return $"{propertyName};";
    }

    private static string MethodToCode(JavaScriptMethod method)
    {
        string methodParams = string.Join(", ", method.Params.Select(JavaScriptVariableExpression.NormalizeName));
        string methodBody = method.Body.ToCode();
        return $"{method.Name}({methodParams}) {{ {methodBody} }}";
    }
}