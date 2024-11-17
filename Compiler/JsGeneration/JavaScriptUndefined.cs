namespace DotNetComp.Compiler.JsGeneration;


public record JavaScriptUndefined() : IJavaScriptStatement
{
    public IJavaScriptStatement AsExpression() => this;
    public bool IsExpression() => true;
    public string ToCode() => "undefined";
}