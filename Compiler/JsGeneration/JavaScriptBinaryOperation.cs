namespace DotNetComp.Compiler.JsGeneration;

public enum JavaScriptBinaryOperator
{
    Add, Sub, Mul, Div, Mod, Lt, Lte, Gt, Gte, And, Or, LogAnd, LogOr, Eq, Ne, Assign,
}

public record JavaScriptBinaryOperation(
    JavaScriptBinaryOperator Operator, 
    IJavaScriptStatement Left, 
    IJavaScriptStatement Right) : IJavaScriptStatement
{
    public IJavaScriptStatement AsExpression() => this;
    public bool IsExpression() => true;

    public string ToCode()
    {
        string left = Left.AsExpression().ToCode();
        string right = Right.AsExpression().ToCode();
        string op = Operator switch
        {
            JavaScriptBinaryOperator.Add => "+",
            JavaScriptBinaryOperator.Sub => "-",
            JavaScriptBinaryOperator.Mul => "*",
            JavaScriptBinaryOperator.Div => "/",
            JavaScriptBinaryOperator.Mod => "%",
            JavaScriptBinaryOperator.Lt => "<",
            JavaScriptBinaryOperator.Lte => "<=",
            JavaScriptBinaryOperator.Gt => ">",
            JavaScriptBinaryOperator.Gte => ">=",
            JavaScriptBinaryOperator.And => "&&",
            JavaScriptBinaryOperator.Or => "||",
            JavaScriptBinaryOperator.LogAnd => "&&",
            JavaScriptBinaryOperator.LogOr => "||",
            JavaScriptBinaryOperator.Eq => "===",
            JavaScriptBinaryOperator.Ne => "!==",
            JavaScriptBinaryOperator.Assign => "=",
            _ => throw new NotImplementedException(),
        };
        return $"{left} {op} {right}";
    }
}