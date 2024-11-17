namespace DotNetComp.Compiler.JsGeneration;

public record JavaScriptWhileLoop(IJavaScriptStatement Condition, IJavaScriptStatement Body)
    : IJavaScriptStatement
{
    public IJavaScriptStatement AsExpression()
    {
        return new JavaScriptIIFE(this);
    }

    public bool IsExpression() => false;

    public string ToCode()
    {
        string condition = Condition.AsExpression().ToCode();
        string body = Body.ToCode();
        return $"while ({condition}) {{ {body}; }}";
    }
}