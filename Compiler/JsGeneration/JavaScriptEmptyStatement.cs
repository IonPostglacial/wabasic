namespace DotNetComp.Compiler.JsGeneration;

public record JavaScriptEmptyStatement : IJavaScriptStatement
{
    public IJavaScriptStatement AsExpression()
    {
        return new JavaScriptUndefined();
    }

    public bool IsExpression() => false;

    public string ToCode() => "";
}