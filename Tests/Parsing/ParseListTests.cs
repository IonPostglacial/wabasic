using DotNetComp.Compiler.Ast;

namespace DotNetComp.Tests.Parsing;

[TestFixture]
class ParseListTests
{
    [TestCaseSource(nameof(ParseListCases))]
    public void TestParseList((string source, INode expected) td)
    {
        ParseTestTools.TestParsedAstNodes(td);
    }

    private static readonly (string, INode)[] ParseListCases =
    [
        ("(1, 2)", new ListLiteral(0..6,
        [
            new NumberLiteral(1..2, 1),
            new NumberLiteral(4..5, 2),
        ])),
    ];
}