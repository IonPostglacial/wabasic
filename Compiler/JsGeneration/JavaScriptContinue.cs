namespace DotNetComp.Compiler.JsGeneration;

public record JavaScriptContinue() : IJavaScriptStatement
{
    public IJavaScriptStatement AsExpression()
    {
        return new SpecialException(Flow.Continue, new JavaScriptUndefined());
    }

    public bool IsExpression() => false;

    public string ToCode()
    {
        return $"continue;";
    }
}