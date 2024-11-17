namespace DotNetComp.Compiler;

using Ast;

public class ParsingError
{
    public interface IError
    {
        public Range Range { get; }
    }

    public record UnexpectedToken(Token Token) : IError
    {
        public Range Range => Token.Range;
    }

    public record ExpectedKeywordButFound(string[] Keywords, Token Found) : IError
    {
        public Range Range => Found.Range;
    }

    public record ExpectedButFound(TokenKind[] Kinds, Token Found) : IError
    {
        public Range Range => Found.Range;
    }

    public record BinaryOperationMissOperand(Token BinaryOperator) : IError
    {
        public Range Range => BinaryOperator.Range;
    }

    public record ExpectedSome(Range Range, TokenKind[] Kinds) : IError;
    public record ExpectedSomeType(Range Range) : IError;

    public record UnexpectedEndAfter(Token Token) : IError
    {
        public Range Range => Token.Range;
    }

    public record AssertExpressionFailed(Range Range, string Message) : IError;
    public record UnexpectedExpression(INode Expression) : IError
    {
        public Range Range => Expression.Range;
    }

    public record UnclosedParens(Token Parens) : IError
    {
        public Range Range => Parens.Range;
    }

    public record ExtraneousParens(Token Parens) : IError
    {
        public Range Range => Parens.Range;
    }

    public record InvalidTypeKind(Range Range, SymKind Kind) : IError;
    
    public record InvalidDim(Token Token) : IError
    {
        public Range Range => Token.Range;
    }

    public record InvalidOperation(Range Range, string Operation) : IError;

    public record InvalidVariableName(Range Range, Symbol Symbol) : IError;
    public record InvalidParameterName(INode Expression) : IError
    {
        public Range Range => Expression.Range;
    }

    public record InvalidToken(Token Token) : IError
    {
        public Range Range => Token.Range;
    }

    public record WrongArgumentsCount(Range Range, List<INode>? Args, IEnumerable<int> Arity) : IError;

    public static (int Line, int Column) CharIndexToLineColumn(string source, int index)
    {
        int lineNo = 1, column = 1;
        for (int i = 0; i < index; i++)
        {
            switch (source[i])
            {
                case '\r':
                    continue;
                case '\n':
                    column = 1;
                    lineNo++;
                    break;
                default:
                    column++;
                    break;
            }
        }
        return (lineNo, column);
    }
}