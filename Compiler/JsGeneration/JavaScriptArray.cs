namespace DotNetComp.Compiler.JsGeneration;

public record JavaScriptArray(IEnumerable<IJavaScriptStatement> Elements) : IJavaScriptStatement
{
    public IJavaScriptStatement AsExpression() => this;

    public bool IsExpression() => true;

    public string ToCode()
    {
        var elements = Elements.Select(e => e.AsExpression().ToCode());
        return $"[{string.Join(", ", elements)}]";
    }
}