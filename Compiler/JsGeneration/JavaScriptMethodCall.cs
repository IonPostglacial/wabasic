namespace DotNetComp.Compiler.JsGeneration;

public record JavaScriptMethodCall(IJavaScriptStatement Callee, string Method, IEnumerable<IJavaScriptStatement> Args) : IJavaScriptStatement
{
    public IJavaScriptStatement AsExpression() => this;

    public bool IsExpression() => true;

    public string ToCode()
    {
        var callee = Callee.AsExpression().ToCode();
        var args = Args.Select(arg => arg.AsExpression().ToCode());
        return $"{callee}.{Method}({string.Join(',', args)})";
    }
}