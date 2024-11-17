namespace DotNetComp.Compiler.JsGeneration;

public record JavaScriptSwitchClause(IJavaScriptStatement Case, IJavaScriptStatement Value);

public record JavaScriptSwitchBreak(
    IJavaScriptStatement Value,
    IEnumerable<JavaScriptSwitchClause> Clauses,
    IJavaScriptStatement? Default) : IJavaScriptStatement
{
    public IJavaScriptStatement AsExpression()
    {
        return new JavaScriptIIFESwitchReturn(Value, Clauses, Default);
    }

    public bool IsExpression() => false;

    public string ToCode()
    {
        var value = Value.AsExpression().ToCode();
        var clauses = Clauses.Select(clause => {
            var caseValue = clause.Case.AsExpression().ToCode();
            var retValue = clause.Value.ToCode();
            return $"case {caseValue}: {retValue}; break;";
        });
        var defaultClause = "";
        if (Default is not null)
        {
            defaultClause = $" default: {Default.ToCode()}; break;";
        }
        return $"switch ({value}) {{ {string.Join(" ", clauses)}{defaultClause} }}";
    }
}