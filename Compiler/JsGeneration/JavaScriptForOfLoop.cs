namespace DotNetComp.Compiler.JsGeneration;

public record JavaScriptForOfLoop(string KeyName, string ValName, IJavaScriptStatement Iterator, IJavaScriptStatement Body)
    : IJavaScriptStatement
{
    public IJavaScriptStatement AsExpression()
    {
        return new JavaScriptIIFE(this);
    }

    public bool IsExpression() => false;

    public string ToCode()
    {
        string iterator = Iterator.AsExpression().ToCode();
        string body = Body.ToCode();
        return $"for ([{KeyName}, {ValName}] of {iterator}) {{ {body}; }}";
    }
}