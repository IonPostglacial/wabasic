namespace DotNetComp.Compiler.JsGeneration;

public record JavaScriptBreak() : IJavaScriptStatement
{
    public IJavaScriptStatement AsExpression()
    {
        return new SpecialException(Flow.Break, new JavaScriptUndefined());
    }

    public bool IsExpression() => false;

    public string ToCode()
    {
        return $"break;";
    }
}