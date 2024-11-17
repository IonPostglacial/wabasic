namespace DotNetComp.Compiler.JsGeneration;

public record JavaScriptIIFE(IJavaScriptStatement Statement) : IJavaScriptStatement
{
    public IJavaScriptStatement AsExpression() => this;

    public bool IsExpression() => true;

    public string ToCode()
    {
        return $"(() => {{{Statement.ToCode()}}})";
    }
}