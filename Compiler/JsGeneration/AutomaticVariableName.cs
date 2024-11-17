namespace DotNetComp.Compiler.JsGeneration;

public class AutomaticVariableName
{
    private readonly HashSet<string> usedLoopVariableNames = [];
    private readonly HashSet<string> variableNames = [];
    private int lastAutoId = 0;

    public void RegisterVariable(string varName)
    {
        variableNames.Add(JavaScriptVariableExpression.NormalizeName(varName));
    }

    public void FreeLoopVariable(string varName)
    {
        usedLoopVariableNames.Remove(varName);
    }

    public string GetVariableDeclarations()
    {
        if (variableNames.Count == 0)
            return "";
        return $"var {string.Join(", ", variableNames)};";
    }

    public string GenerateVariableName()
    {
        string autoVarName = $"_{lastAutoId++}$";
        variableNames.Add(autoVarName);
        return autoVarName;
    }

    public string GenerateLoopVariableName()
    {
        foreach (var name in JavaScriptVariableExpression.LoopVariableNames)
        {
            if (!usedLoopVariableNames.Contains(name) && !variableNames.Contains(name))
            {
                usedLoopVariableNames.Add(name);
                return name;
            }
        }
        return GenerateVariableName();
    }
}