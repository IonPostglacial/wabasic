using DotNetComp.Compiler.Ast;

namespace DotNetComp.Tests.Parsing;


[TestFixture]
class ParseCaseStatementTests
{
    [TestCaseSource(nameof(ParseCaseStatementTestCases))]
    public void TestParseCaseStatement((string source, INode expected) td)
    {
        ParseTestTools.TestParsedAstNodes(td);
    }

    private static readonly (string, INode)[] ParseCaseStatementTestCases =
    [
        ("case()", new InvalidNode(0..6)),
        ("case(1)", new InvalidNode(0..7)),
        ("case(1, 2)", new InvalidNode(0..10)),
        ("case(1, 2, 3)", new CaseStatement(0..13, new NumberLiteral(5..6, 1),
            [
                new CaseStatement.Clause(new NumberLiteral(8..9, 2), new NumberLiteral(11..12, 3))
            ], null)),
        ("case(1, 2, 3, 4, 5)", new CaseStatement(0..19, new NumberLiteral(5..6, 1),
            [
                new CaseStatement.Clause(new NumberLiteral(8..9, 2), new NumberLiteral(11..12, 3)),
                new CaseStatement.Clause(new NumberLiteral(14..15, 4), new NumberLiteral(17..18, 5))
            ], null)),
        ("case(1, 2, 3, 4, 5, 6)", new CaseStatement(0..22, new NumberLiteral(5..6, 1),
            [
                new CaseStatement.Clause(new NumberLiteral(8..9, 2), new NumberLiteral(11..12, 3)),
                new CaseStatement.Clause(new NumberLiteral(14..15, 4), new NumberLiteral(17..18, 5))
            ], new NumberLiteral(20..21, 6))),
    ];
}