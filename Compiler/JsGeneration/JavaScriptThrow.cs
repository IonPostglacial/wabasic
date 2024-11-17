namespace DotNetComp.Compiler.JsGeneration;

public record JavaScriptThrow(IJavaScriptStatement Expression) : IJavaScriptStatement
{
    public IJavaScriptStatement AsExpression() => new JavaScriptIIFE(this);

    public bool IsExpression() => false;

    public string ToCode()
    {
        return $"throw {Expression.AsExpression().ToCode()};";
    }
}