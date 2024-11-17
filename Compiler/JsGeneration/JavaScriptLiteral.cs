using System.Text.Json;

namespace DotNetComp.Compiler.JsGeneration;

public record JavaScriptLiteral(object Value) : IJavaScriptStatement
{
    public IJavaScriptStatement AsExpression() => this;

    public bool IsExpression() => true;

    public string ToCode()
    {
        return JsonSerializer.Serialize(Value);
    }
}