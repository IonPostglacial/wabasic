namespace DotNetComp.Compiler.JsGeneration;

public record JavaScriptIfStatement(IJavaScriptStatement Condition, IJavaScriptStatement Then, IJavaScriptStatement? Else)
    : IJavaScriptStatement
{
    public IJavaScriptStatement AsExpression()
    {
        return new JavaScriptTernaryExpression(Condition, Then, Else ?? new JavaScriptUndefined());
    }

    public bool IsExpression() => false;

    public string ToCode()
    {
        string condition = Condition.AsExpression().ToCode();
        string then = Then.ToCode();
        if (Else is null)
        {
            return $"if ({condition}) {{ {then}; }}";
        }
        else
        {
            string elseCode = Else.ToCode();
            return $"if ({condition}) {{ {then}; }} else {{ {elseCode}; }}";
        }
    }
}