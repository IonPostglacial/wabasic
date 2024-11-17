namespace DotNetComp.Compiler.JsGeneration;

public record JavaScriptTernaryExpression(IJavaScriptStatement Condition, IJavaScriptStatement Then, IJavaScriptStatement Else)
    : IJavaScriptStatement
{
    public IJavaScriptStatement AsExpression() => this;

    public bool IsExpression() => true;

    public string ToCode()
    {
        string condition = Condition.AsExpression().ToCode();
        string then = Then.AsExpression().ToCode();
        string elseCode = Else.AsExpression().ToCode();
        return $"({condition} ? {then} : {elseCode})";
    }
}