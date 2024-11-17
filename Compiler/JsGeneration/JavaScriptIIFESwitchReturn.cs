namespace DotNetComp.Compiler.JsGeneration;


public record JavaScriptIIFESwitchReturn(
    IJavaScriptStatement Value,
    IEnumerable<JavaScriptSwitchClause> Clauses,
    IJavaScriptStatement? Default) : IJavaScriptStatement
{
    public IJavaScriptStatement AsExpression() => this;

    public bool IsExpression() => true;

    public string ToCode()
    {
        var value = Value.AsExpression().ToCode();
        var clauses = Clauses.Select(clause => {
            var caseValue = clause.Case.AsExpression().ToCode();
            var retValue = clause.Value.AsExpression().ToCode();
            return $"case {caseValue}: return {retValue};";
        });
        var defaultClause = "";
        if (Default is not null)
        {
            defaultClause = $" default: {Default.AsExpression().ToCode()}; break;";
        }
        return $"(() => {{ switch ({value}) {{ {string.Join(" ", clauses)}{defaultClause} }} }})()";
    }
}