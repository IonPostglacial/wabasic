using DotNetComp.Compiler;
using DotNetComp.Compiler.Ast;

namespace DotNetComp.Tests.Parsing;

[TestFixture]
class ParseSelectTests
{
    [TestCaseSource(nameof(ParseSelectCases))]
    public void TestParseSelect((string source, INode expected) td)
    {
        ParseTestTools.TestParsedAstNodes(td);
    }

    private static readonly (string, INode)[] ParseSelectCases =
    [
        ("f[]", new SelectCall(0..3, SelectCall.Kind.Primary,
            new Variable(0..1, Symbol.BuiltIn("f")),
            [])),
        ("f[1]", new SelectCall(0..4, SelectCall.Kind.Primary,
            new Variable(0..1, Symbol.BuiltIn("f")),
            [new NumberLiteral(2..3, 1)])),
        ("f²[1]", new SelectCall(0..5, SelectCall.Kind.Secondary,
            new Variable(0..1, Symbol.BuiltIn("f")),
            [new NumberLiteral(3..4, 1)])),
        ("f`[1]", new SelectCall(0..5, SelectCall.Kind.Secondary,
            new Variable(0..1, Symbol.BuiltIn("f")),
            [new NumberLiteral(3..4, 1)])),
        ("f[1, 2]", new SelectCall(0..7, SelectCall.Kind.Primary,
            new Variable(0..1, Symbol.BuiltIn("f")),
            [
                new NumberLiteral(2..3, 1),
                new NumberLiteral(5..6, 2)
            ])),
    ];
}