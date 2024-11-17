using DotNetComp.Compiler.Ast;

namespace DotNetComp.Tests.Parsing;

[TestFixture]
class ParseCallableStatementsTests
{
    [TestCaseSource(nameof(ParseStatementsCases))]
    public void TestParseStatements((string source, INode expected) td)
    {
        ParseTestTools.TestParsedAstNodes(td);
    }

    private static readonly (string, INode)[] ParseStatementsCases =
    [
        ("return(1)", new ReturnStatement(0..9,
            new NumberLiteral(7..8, 1))),
        ("if(1, 2)", new IfStatement(0..8,
            new NumberLiteral(3..4, 1),
            new NumberLiteral(6..7, 2),
            null)),
        ("if(1, 2, 3)", new IfStatement(0..11,
            new NumberLiteral(3..4, 1),
            new NumberLiteral(6..7, 2),
            new NumberLiteral(9..10, 3))),
        ("for(1, 2)", new ForLoop(0..9,
            new NumberLiteral(4..5, 1),
            new NumberLiteral(7..8, 2))),
        ("foreach(1, 2)", new ForEachLoop(0..13,
            new NumberLiteral(8..9, 1),
            new NumberLiteral(11..12, 2))),
        ("while(1, 2)", new WhileLoop(0..11,
            new NumberLiteral(6..7, 1),
            new NumberLiteral(9..10, 2))),
    ];
}