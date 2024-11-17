namespace DotNetComp.Compiler.JsGeneration;

public record JavaScriptNewConstructor(IJavaScriptStatement Constructor, IEnumerable<IJavaScriptStatement> Args) : IJavaScriptStatement
{
    public IJavaScriptStatement AsExpression() => this;

    public bool IsExpression() => true;

    public string ToCode()
    {
        var callee = Constructor.AsExpression().ToCode();
        var args = Args.Select(arg => arg.AsExpression().ToCode());
        return $"new {callee}({string.Join(',', args)})";
    }
}