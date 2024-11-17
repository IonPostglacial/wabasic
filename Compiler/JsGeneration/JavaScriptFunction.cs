namespace DotNetComp.Compiler.JsGeneration;

public class JavaScriptFunction(IEnumerable<string> Params, IJavaScriptStatement Body) : IJavaScriptStatement
{
    public IJavaScriptStatement AsExpression() => this;

    public bool IsExpression() => true;

    public string ToCode()
    {
        string body;
        if (Body is JavaScriptStatementSequence seq)
            body = seq.EndingWithReturn().ToCode();
        else
            body = new JavaScriptReturn(Body).ToCode();
        return $"(({string.Join(", ", Params)}) => {{ {body} }})";
    }
}