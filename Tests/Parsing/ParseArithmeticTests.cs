using DotNetComp.Compiler;
using DotNetComp.Compiler.Ast;

namespace DotNetComp.Tests.Parsing;

[TestFixture]
class ParseArithmeticTests
{
    [TestCaseSource(nameof(ParseArithmeticCases))]
    public void TestArithmetic((string source, INode expected) td)
    {
        ParseTestTools.TestParsedAstNodes(td);
    }

    private static readonly (string, INode)[] ParseArithmeticCases =
    [
        ("1 + 2", new BinaryOperation(
            BinaryOperator.Add,
            new NumberLiteral(0..1, 1),
            new NumberLiteral(4..5, 2))),
        ("(1 + 2)", new BinaryOperation(
            BinaryOperator.Add,
            new NumberLiteral(1..2, 1),
            new NumberLiteral(5..6, 2))),
        ("1 + 2 + 3", new BinaryOperation(
            BinaryOperator.Add,
            new BinaryOperation(
                BinaryOperator.Add,
                new NumberLiteral(0..1, 1),
                new NumberLiteral(4..5, 2)),
            new NumberLiteral(8..9, 3)
        )),
        ("1 + (2 + 3)", new BinaryOperation(
            BinaryOperator.Add,
            new NumberLiteral(0..1, 1),
            new BinaryOperation(
                BinaryOperator.Add,
                new NumberLiteral(5..6, 2),
                new NumberLiteral(9..10, 3)))
        ),
        ("1 + 2 * 3", new BinaryOperation(
            BinaryOperator.Add,
            new NumberLiteral(0..1, 1),
            new BinaryOperation(
                BinaryOperator.Mul,
                new NumberLiteral(4..5, 2),
                new NumberLiteral(8..9, 3)))
        ),
        ("1 or 2 and 3", new BinaryOperation(
            BinaryOperator.Or,
            new NumberLiteral(0..1, 1),
            new BinaryOperation(
                BinaryOperator.And,
                new NumberLiteral(5..6, 2),
                new NumberLiteral(11..12, 3)))
        ),
        ("1 + 2 * 3 + 4", new BinaryOperation(
            BinaryOperator.Add,
            new BinaryOperation(
                BinaryOperator.Add,
                new NumberLiteral(0..1, 1),
                new BinaryOperation(
                    BinaryOperator.Mul,
                    new NumberLiteral(4..5, 2),
                    new NumberLiteral(8..9, 3))),
            new NumberLiteral(12..13, 4))
        ),
        ("1 * 2 + 3 * 4", new BinaryOperation(
            BinaryOperator.Add,
            new BinaryOperation(
                BinaryOperator.Mul,
                new NumberLiteral(0..1, 1),
                new NumberLiteral(4..5, 2)),
            new BinaryOperation(
                BinaryOperator.Mul,
                new NumberLiteral(8..9, 3),
                new NumberLiteral(12..13, 4)))
        ),
        ("not true", new Negation(0..8, new Variable(4..8, Symbol.BuiltIn("true")))),
        ("-$x", new UnaryMinus(0..3, new Variable(1..3, Symbol.Local("x"))))
    ];
}