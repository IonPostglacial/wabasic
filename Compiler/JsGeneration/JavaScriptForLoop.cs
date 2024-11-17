namespace DotNetComp.Compiler.JsGeneration;

public record JavaScriptForLoop(IJavaScriptStatement Init, IJavaScriptStatement Condition, IJavaScriptStatement Each, IJavaScriptStatement Body)
    : IJavaScriptStatement
{
    public IJavaScriptStatement AsExpression()
    {
        return new JavaScriptIIFE(this);
    }

    public bool IsExpression() => false;

    public string ToCode()
    {
        string init = Init.ToCode();
        string condition = Condition.AsExpression().ToCode();
        string each = Each.ToCode();
        string body = Body.ToCode();
        return $"for ({init}; {condition}; {each}) {{ {body}; }}";
    }
}