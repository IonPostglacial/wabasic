namespace DotNetComp.Compiler.JsGeneration;

public record JavaScriptVariableExpression(string Name, bool LoopVariable = false) : IJavaScriptStatement
{
    public static readonly string[] LoopVariableNames = ["i", "j", "k"];

    public static string NormalizeName(string varName)
    {
        varName = varName.ToLower();
        if (LoopVariableNames.Contains(varName))
        {
            varName = $"{varName}$";
        }
        return varName;
    }

    public IJavaScriptStatement AsExpression() => this;

    public bool IsExpression() => true;

    public string ToCode()
    {
        if (LoopVariable)
            return Name;
        return NormalizeName(Name);
    }
}