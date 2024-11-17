namespace DotNetComp.Compiler.JsGeneration;

public enum Flow { Break, Continue, Return }

public record SpecialException(Flow Flow, IJavaScriptStatement Value) : IJavaScriptStatement
{
    public IJavaScriptStatement AsExpression() => this;

    public bool IsExpression() => true;

    public string ToCode()
    {
        string flow = Flow switch
        {
            Flow.Break => "break",
            Flow.Continue => "continue",
            Flow.Return => "return",
            _ => "?",
        };
        string value = Value.ToCode();
        return $"{{ flow: \"{flow}\", value: {value} }}";
    }
}