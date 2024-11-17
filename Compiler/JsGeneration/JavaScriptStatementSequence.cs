namespace DotNetComp.Compiler.JsGeneration;

public record JavaScriptStatementSequence(IEnumerable<IJavaScriptStatement> Statements) : IJavaScriptStatement
{
    public IJavaScriptStatement EndingWithReturn()
    {
        if (!Statements.Any())
            return new JavaScriptUndefined();
        var statements = new Stack<IJavaScriptStatement>(Statements);
        var lastStatement = statements.Pop();
        var returnStatement = new JavaScriptReturn(lastStatement);
        statements.Push(returnStatement);
        return new JavaScriptStatementSequence(statements.Reverse());
    }

    public IJavaScriptStatement AsExpression()
    {
        var statements = EndingWithReturn();
        return new JavaScriptIIFE(statements);
    }

    public bool IsExpression() => false;

    public string ToCode()
    {
        return string.Join("; ", Statements.Select(s => s.ToCode()));
    }
}