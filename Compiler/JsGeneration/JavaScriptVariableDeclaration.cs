namespace DotNetComp.Compiler.JsGeneration;

public record JavaScriptVariableDeclaration(string Name, IJavaScriptStatement? Value, bool LoopVariable = false) : IJavaScriptStatement
{
    public IJavaScriptStatement AsExpression()
    {
        throw new NotImplementedException();
    }

    public bool IsExpression() => false;

    public string ToCode()
    {
        string name = LoopVariable ? Name : JavaScriptVariableExpression.NormalizeName(Name);
        string value = "";
        if (Value is not null)
        {
            value = $" = {Value.AsExpression().ToCode()}";
        }
        return $"var {name}{value}";
    }
}