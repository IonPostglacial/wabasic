namespace DotNetComp.Compiler.JsGeneration;

public record JavaScriptAccessProperty(IJavaScriptStatement Callee, string Property) : IJavaScriptStatement
{
    public IJavaScriptStatement AsExpression() => this;

    public bool IsExpression() => true;

    public string ToCode()
    {
        var callee = Callee.AsExpression().ToCode();
        return $"{callee}.{Property}";
    }
}