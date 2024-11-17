namespace DotNetComp.Compiler;

public enum BinaryOperator
{
    Eq, Ne, Lt, Gt, Lte, Gte, 
    Cat, Add, Sub, Mul, Div, Pow,
    In, Or, OrElse, And, AndAlso, BitOr, BitAnd,
    Invalid
}

public static class BinaryOperatorTools
{
    private readonly static string[] _ops = [
        "=", "!=", "<", ">", "<=", ">=", 
        "&", "+", "-", "*", "/", "^",
        "in", "or", "orelse", "and", "andalso", "bitor", "bitand"
    ];
    public static bool IsOperator(ReadOnlySpan<char> tok)
    {
        foreach (string op in _ops)
        {
            if (op.AsSpan().Equals(tok, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
    }

        
    public static BinaryOperator OperatorFromText(ReadOnlySpan<char> tok)
    {
        foreach (var (op, i) in _ops.Select((op, i) => (op, i)))
        {
            if (op.AsSpan().Equals(tok, StringComparison.OrdinalIgnoreCase))
            {
                return (BinaryOperator)i;
            }
        }
        return BinaryOperator.Invalid;
    }

    public static (int left, int right) BindingPower(this BinaryOperator op)
    {
        return op switch
        {
            BinaryOperator.In => (1, 2),
            BinaryOperator.Or or BinaryOperator.OrElse => (3, 4),
            BinaryOperator.And or BinaryOperator.AndAlso => (5, 6),
            BinaryOperator.BitOr => (7, 8),
            BinaryOperator.BitAnd => (9, 10),
            BinaryOperator.Lt or BinaryOperator.Gt or BinaryOperator.Lte or BinaryOperator.Gte => (11, 12),
            BinaryOperator.Eq or BinaryOperator.Ne => (14, 13),
            BinaryOperator.Cat or BinaryOperator.Add or BinaryOperator.Sub => (15, 16),
            BinaryOperator.Mul or BinaryOperator.Div => (17, 18),
            BinaryOperator.Pow => (19, 20),
            _ => (0, 0),
        };
    }
}