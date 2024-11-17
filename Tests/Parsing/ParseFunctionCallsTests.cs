using DotNetComp.Compiler;
using DotNetComp.Compiler.Ast;

namespace DotNetComp.Tests.Parsing;

[TestFixture]
class ParseFunctionCallsTests
{
    [TestCaseSource(nameof(ParseFunctionCallsCases))]
    public void TestParseFunctionCalls((string source, INode expected) td)
    {
        ParseTestTools.TestParsedAstNodes(td);
    }

    private static readonly (string, INode)[] ParseFunctionCallsCases =
    [
        ("f()", new FunctionCall(0..3,
            new Variable(0..1, Symbol.BuiltIn("f")),
            [])),
        ("f(1)", new FunctionCall(0..4,
            new Variable(0..1, Symbol.BuiltIn("f")),
            [new NumberLiteral(2..3, 1)])),
        ("#f(1)", new FunctionCall(0..5,
            new Variable(0..2, new Symbol(SymKind.Constant, "", "f")),
            [new NumberLiteral(3..4, 1)])),
        ("f(g(1), 2)", new FunctionCall(0..10,
            new Variable(0..1, Symbol.BuiltIn("f")),
            [
                new FunctionCall(2..6,
                    new Variable(2..3, Symbol.BuiltIn("g")),
                    [new NumberLiteral(4..5, 1)]),
                new NumberLiteral(8..9, 2)
            ])),
        ("f(1, 2)", new FunctionCall(0..7,
            new Variable(0..1, Symbol.BuiltIn("f")),
            [
                new NumberLiteral(2..3, 1),
                new NumberLiteral(5..6, 2)
            ])),
    ];
}