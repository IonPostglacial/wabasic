namespace DotNetComp.Compiler.JsGeneration;

public record JavaScriptCall(IJavaScriptStatement Callee, IEnumerable<IJavaScriptStatement> Args) : IJavaScriptStatement
{
    public IJavaScriptStatement AsExpression() => this;

    public bool IsExpression() => true;

    public string ToCode()
    {
        var callee = Callee.AsExpression().ToCode();
        var args = Args.Select(arg => arg.AsExpression().ToCode());
        return $"{callee}({string.Join(',', args)})";
    }
}