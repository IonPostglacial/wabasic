namespace DotNetComp.Compiler.JsGeneration;

public interface IJavaScriptStatement
{
    string ToCode();
    bool IsExpression();
    IJavaScriptStatement AsExpression();
}