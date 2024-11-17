using DotNetComp.Compiler;

namespace DotNetComp.Tests.Parsing;

[TestFixture]
public class TokenizerTests
{
    [SetUp]
    public void Setup()
    {
    }

    [TestCase("0", TokenKind.Num, 0, 1)]
    [TestCase("1", TokenKind.Num, 0, 1)]
    [TestCase("01", TokenKind.Num, 0, 2)]
    [TestCase("1.0", TokenKind.Num, 0, 3)]
    [TestCase("01.01", TokenKind.Num, 0, 5)]

    [TestCase("\"\"", TokenKind.Str, 0, 2)]
    [TestCase("\" \"", TokenKind.Str, 0, 3)]
    [TestCase("\"\"\"\"", TokenKind.Str, 0, 4)]
    [TestCase("\"hello\"", TokenKind.Str, 0, 7)]

    [TestCase("+", TokenKind.Operator, 0, 1)]
    [TestCase("<", TokenKind.Operator, 0, 1)]
    [TestCase("!=", TokenKind.Operator, 0, 2)]
    [TestCase("and", TokenKind.Operator, 0, 3)]
    [TestCase("AND", TokenKind.Operator, 0, 3)]
    [TestCase("And", TokenKind.Operator, 0, 3)]

    [TestCase("=>", TokenKind.FatArrow, 0, 2)]
    [TestCase(":=", TokenKind.Assign, 0, 2)]
    [TestCase("not", TokenKind.Not, 0, 3)]
    [TestCase(".", TokenKind.Dot, 0, 1)]
    [TestCase("new", TokenKind.New, 0, 3)]
    [TestCase("class", TokenKind.Class, 0, 5)]

    [TestCase("(", TokenKind.OpenParens, 0, 1)]
    [TestCase(")", TokenKind.CloseParens, 0, 1)]
    [TestCase("[", TokenKind.OpenSquare, 0, 1)]
    [TestCase("}", TokenKind.CloseBracket, 0, 1)]
    
    [TestCase("a", TokenKind.Sym, 0, 1)]
    [TestCase("ab", TokenKind.Sym, 0, 2)]
    [TestCase("$a", TokenKind.Sym, 0, 2)]
    [TestCase("$hello", TokenKind.Sym, 0, 6)]
    [TestCase("#cruxe", TokenKind.Sym, 0, 6)]
    [TestCase("@balad", TokenKind.Sym, 0, 6)]
    public void SingleToken(string source, TokenKind kind, int start, int end)
    {
        Tokenizer tokenizer = new (source);
        Token token = tokenizer.Next();
        Assert.Multiple(() =>
        {
            Assert.That(token.Kind, Is.EqualTo(kind));
            Assert.That(token.Range, Is.EqualTo(start..end));
        });
    }

    [TestCaseSource(nameof(MultipleTokenCases))]
    public void MultipleToken((string source, Token[] expected) td)
    {
        Tokenizer tokenizer = new (td.source);
        Token tok = tokenizer.Next();
        int i = 0;
        while (tok.Kind != TokenKind.Eof)
        {
            Assert.Multiple(() =>
            {
                Assert.That(i, Is.LessThan(td.expected.Length));
                if (i < td.expected.Length)
                    Assert.That(tok, Is.EqualTo(td.expected[i]));
            });
            i++;
            tok = tokenizer.Next();
        }
        Assert.That(i, Is.EqualTo(td.expected.Length));
    }

    private static readonly (string, Token[])[] MultipleTokenCases =
    [
        ("1", [new (TokenKind.Num, 0..1)]),
        ("-1", [new (TokenKind.Operator, 0..1), new (TokenKind.Num, 1..2)]),
        ("1 + 1", [new (TokenKind.Num, 0..1), new (TokenKind.Operator, 2..3), new (TokenKind.Num, 4..5)]),
        ("1*1", [new (TokenKind.Num, 0..1), new (TokenKind.Operator, 1..2), new (TokenKind.Num, 2..3)]),
        ("()=>(1)", [
            new (TokenKind.OpenParens, 0..1), 
            new (TokenKind.CloseParens, 1..2),
            new (TokenKind.FatArrow, 2..4), 
            new (TokenKind.OpenParens, 4..5),
            new (TokenKind.Num, 5..6), 
            new (TokenKind.CloseParens, 6..7)]),
        ("hello[world]", [
            new (TokenKind.Sym, 0..5), 
            new (TokenKind.OpenSquare, 5..6),
            new (TokenKind.Sym, 6..11),
            new (TokenKind.CloseSquare, 11..12),
        ]),
        ("If(True,1,2)", [
            new (TokenKind.Sym, 0..2), 
            new (TokenKind.OpenParens, 2..3),
            new (TokenKind.Sym, 3..7),
            new (TokenKind.Comma, 7..8),
            new (TokenKind.Num, 8..9),
            new (TokenKind.Comma, 9..10),
            new (TokenKind.Num, 10..11),
            new (TokenKind.CloseParens, 11..12),
        ]),
        ("#mama(world)", [
            new (TokenKind.Sym, 0..5), 
            new (TokenKind.OpenParens, 5..6),
            new (TokenKind.Sym, 6..11),
            new (TokenKind.CloseParens, 11..12),
        ]),
        ("If(0,1;2,3;4);5", [
            new (TokenKind.Sym, 0..2), 
            new (TokenKind.OpenParens, 2..3),
            new (TokenKind.Num, 3..4),
            new (TokenKind.Comma, 4..5),
            new (TokenKind.Num, 5..6),
            new (TokenKind.Sep, 6..7),
            new (TokenKind.Num, 7..8),
            new (TokenKind.Comma, 8..9),
            new (TokenKind.Num, 9..10),
            new (TokenKind.Sep, 10..11),
            new (TokenKind.Num, 11..12),
            new (TokenKind.CloseParens, 12..13),
            new (TokenKind.Sep, 13..14),
            new (TokenKind.Num, 14..15),
        ]),
        ("T{1}", [
            new (TokenKind.Sym, 0..1),
            new (TokenKind.OpenBracket, 1..2),
            new (TokenKind.Num, 2..3),
            new (TokenKind.CloseBracket, 3..4),
        ]),
        ("T.Count{U>3}", [
            new (TokenKind.Sym, 0..1),
            new (TokenKind.Dot, 1..2),
            new (TokenKind.Sym, 2..7),
            new (TokenKind.OpenBracket, 7..8),
            new (TokenKind.Sym, 8..9),
            new (TokenKind.Operator, 9..10),
            new (TokenKind.Num, 10..11),
            new (TokenKind.CloseBracket, 11..12),
        ]),
    ];
}