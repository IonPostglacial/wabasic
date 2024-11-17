using DotNetComp.Compiler;
using DotNetComp.Compiler.Ast;

namespace DotNetComp.Tests.Parsing;

[TestFixture]
class ParseAggregationTests
{
    [TestCaseSource(nameof(ParseAggregationCases))]
    public void TestParseAggregation((string source, INode expected) td)
    {
        ParseTestTools.TestParsedAstNodes(td);
    }

    private static readonly (string, INode)[] ParseAggregationCases =
    [
        ("T.Count{1}", new AggregateCall(0..10,
            new Variable(0..1, Symbol.BuiltIn("T")),
            Symbol.BuiltIn("count"),
            new NumberLiteral(8..9, 1))),
    ];
}