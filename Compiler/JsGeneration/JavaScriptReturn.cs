namespace DotNetComp.Compiler.JsGeneration;

public record JavaScriptReturn(IJavaScriptStatement Expression) : IJavaScriptStatement
{
    public IJavaScriptStatement AsExpression()
    {
        return new SpecialException(Flow.Return, Expression);
    }

    public bool IsExpression() => false;

    public string ToCode()
    {
        return $"return {Expression.AsExpression().ToCode()};";
    }
}