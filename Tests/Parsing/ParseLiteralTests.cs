using DotNetComp.Compiler;
using DotNetComp.Compiler.Ast;

namespace DotNetComp.Tests.Parsing;

[TestFixture]
class ParseLiteralTests
{
    [TestCaseSource(nameof(ParseLiteralCases))]
    public void TestParseLiteral((string source, INode expected) td)
    {
        ParseTestTools.TestParsedAstNodes(td);
    }

    private static readonly (string, INode)[] ParseLiteralCases =
    [
        ("1", new NumberLiteral(0..1, 1)),
        ("1.1", new NumberLiteral(0..3, 1.1)),
        ("0.1", new NumberLiteral(0..3, 0.1)),

        ("\"\"", new StringLiteral(0..2, "")),
        ("\" \"", new StringLiteral(0..3, " ")),
        ("\"\"\"\"", new StringLiteral(0..4, "\"")),
        ("\"hello\"", new StringLiteral(0..7, "hello")),
    ];
}