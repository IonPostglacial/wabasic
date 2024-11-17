namespace DotNetComp.Compiler.JsGeneration;

public enum JavaScriptUnaryOperator { Not, Minus }

public record JavaScriptUnaryOperation(JavaScriptUnaryOperator Operator, IJavaScriptStatement Statement) : IJavaScriptStatement
{
    public IJavaScriptStatement AsExpression() => this;

    public bool IsExpression() => true;

    public string ToCode()
    {
        string stmt = Statement.AsExpression().ToCode();
        string op = Operator switch
        {
            JavaScriptUnaryOperator.Not => "!",
            JavaScriptUnaryOperator.Minus => "-",
            _ => throw new NotImplementedException(),
        };
        return $"{op}{stmt}";
    }
}