using System.Globalization;
using System.Text.RegularExpressions;

namespace DotNetComp.Compiler;

public enum TokenKind
{
    Num, Str,
    Sep, Comma, Dot,
    OpenParens, CloseParens, 
    OpenSquare, OpenSecondSquare, CloseSquare,
    OpenBracket, CloseBracket,
    Let,
    As,
    New,
    Not,
    Assign,
    FatArrow,
    Operator,
    Sym,
    Err,
    Class,
    Eof,
}

public record struct Token(TokenKind Kind, Range Range);
public record Time(int Hour, int Minute, double Seconds);

public partial class Tokenizer(string source)
{
    readonly char[] Separators = [';', ',', '(', ')', '[', ']', '{', '}', ' ', '\t', '\r', '\n',
        '=', '<', '>', '&', '+', '-', '*', '/', '.', '²', '`', ':'];
    readonly char[] Operators = [';', ',', '(', ')', '[', ']', '{', '}', '=', '<', '>', '&', '+', '-', '*', '/', '^', '.', '²', '`'];
    readonly string[] TwoCharOperators = [":=", "<=", ">=", "=>", "`[", "²[", "!="];
    private readonly string _src = source;
    private int _pos = 0;
    private Token? _peeked = null;

    private bool HasNextChar { get => _pos < _src.Length - 1; }
    private char CurrentChar { get => _src[_pos]; }
    private char NextChar { get => _src[_pos + 1]; }

    private void ConsumeChar()
    {
        _pos++;
    }

    private void ConsumeUntilChar(char c)
    {
        do
        {
            ConsumeChar();
        }
        while (CurrentChar != c && HasNextChar);
    }

    private Token ConsumeString()
    {
        int tokStart = _pos;
        ConsumeUntilChar('"');
        while (CurrentChar == '"' && HasNextChar && NextChar == '"')
        {
            ConsumeChar();
            ConsumeUntilChar('"');
        }
        ConsumeChar();
        int tokEnd = _pos;
        return new(TokenKind.Str, new Range(tokStart, tokEnd));
    }

    public Token ConsumeSymbol()
    {
        int tokStart = _pos;
        while (_pos < _src.Length && !Separators.Contains(CurrentChar))
        {
            ConsumeChar();
        }
        int tokEnd = _pos;
        string sym = _src[tokStart..tokEnd];
        TokenKind kind;
        if (BinaryOperatorTools.IsOperator(sym))
            kind = TokenKind.Operator;
        else if (sym.Equals("not", StringComparison.OrdinalIgnoreCase))
            kind = TokenKind.Not;
        else if (sym.Equals("let", StringComparison.OrdinalIgnoreCase))
            kind = TokenKind.Let;
        else if (sym.Equals("as", StringComparison.OrdinalIgnoreCase))
            kind = TokenKind.As;
        else if (sym.Equals("new", StringComparison.OrdinalIgnoreCase))
            kind = TokenKind.New;
        else if (sym.Equals("class", StringComparison.OrdinalIgnoreCase))
            kind = TokenKind.Class;
        else
            kind = TokenKind.Sym;
        return new(kind, new Range(tokStart, tokEnd));
    }

    private static bool IsNumeric(char c) => c is >= '0' and <= '9';

    private Token ConsumeNumber()
    {
        int tokStart = _pos;
        while (HasNextChar && (NextChar == '.' || IsNumeric(NextChar)))
        {
            ConsumeChar();
        }
        ConsumeChar();
        int tokEnd = _pos;
        return new(TokenKind.Num, new Range(tokStart, tokEnd));
    }

    private Token ConsumeOperator()
    {
        int tokStart = _pos;
        int tokEnd = _pos + 1;
        if (tokStart + 2 <= _src.Length && TwoCharOperators.Contains(_src[tokStart..(tokStart + 2)]))
        {
            ConsumeChar();
            tokEnd++;
        }
        string op = _src[tokStart..tokEnd];
        TokenKind kind;
        if (BinaryOperatorTools.IsOperator(op))
            kind = TokenKind.Operator;
        else if (op == "(")
            kind = TokenKind.OpenParens;
        else if (op == ")")
            kind = TokenKind.CloseParens;
        else if (op == "[")
            kind = TokenKind.OpenSquare;
        else if (op == "²[" || op == "`[")
            kind = TokenKind.OpenSecondSquare;
        else if (op == "]")
            kind = TokenKind.CloseSquare;
        else if (op == "{")
            kind = TokenKind.OpenBracket;
        else if (op == "}")
            kind = TokenKind.CloseBracket;
        else if (op == ",")
            kind = TokenKind.Comma;
        else if (op == ".")
            kind = TokenKind.Dot;
        else if (op == ";")
            kind = TokenKind.Sep;
        else if (op == ":=")
            kind = TokenKind.Assign;
        else if (op == "=>")
            kind = TokenKind.FatArrow;
        else
            kind = TokenKind.Operator;
        ConsumeChar();
        return new(kind, new Range(tokStart, tokEnd));
    }

    public Token Peek()
    {
        if (_peeked is Token tok)
            return tok;
        Token next = Next();
        _peeked = next;
        return next;
    }

    public Token Next()
    {
        if (_peeked is Token tok)
        {
            _peeked = null;
            return tok;
        }
        while (_pos < _src.Length)
        {
            switch (CurrentChar)
            {
                case '\n':
                case '\r':
                case '\t':
                case ' ':
                    ConsumeChar();
                    continue;
                case '\'':
                    ConsumeUntilChar('\n');
                    return Next();
                case '"':
                    return ConsumeString();
                default:
                    if (IsNumeric(CurrentChar))
                        return ConsumeNumber();
                    else if (CurrentChar is ':' or '!' || Operators.Contains(CurrentChar))
                        return ConsumeOperator();
                    else
                        return ConsumeSymbol();
            }
        }
        return new Token(TokenKind.Eof, new Range(_src.Length-1, _src.Length-1));
    }

    public ReadOnlySpan<char> AsSpan(Token Token)
    {
        return _src.AsSpan()[Token.Range];
    }

    public string AsString(Token Token)
    {
        return _src[(Token.Range.Start.Value+1)..(Token.Range.End.Value-1)].Replace("\"\"", "\"");
    }

    public Symbol AsSymbol(Token Token)
    {
        return Symbol.FromText(_src.AsSpan()[Token.Range]);
    }

    public double AsNumber(Token Token)
    {
        return double.Parse(_src.AsSpan()[Token.Range], NumberStyles.Any, CultureInfo.InvariantCulture);
    }

    public bool TokenIsSeparator(Token Token)
    {
        ReadOnlySpan<char> tok = _src.AsSpan()[Token.Range];
        return tok.Length > 0 && Separators.Contains(tok[0]); 
    }

    public BinaryOperator TokenToBinaryOperator(Token Token)
    {
        return BinaryOperatorTools.OperatorFromText(_src.AsSpan()[Token.Range]);
    }
}