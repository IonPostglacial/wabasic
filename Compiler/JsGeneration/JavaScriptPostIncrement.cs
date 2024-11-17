namespace DotNetComp.Compiler.JsGeneration;

public record JavaScriptPostIncrement(IJavaScriptStatement Expression) : IJavaScriptStatement
{
    public IJavaScriptStatement AsExpression() => this;

    public bool IsExpression() => true;

    public string ToCode()
    {
        string expression = Expression.AsExpression().ToCode();
        return $"{expression}++";
    }
}