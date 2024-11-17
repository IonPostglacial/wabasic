using DotNetComp.Compiler.Ast;
using DotNetComp.Compiler.Builtin;

namespace DotNetComp.Tests.Parsing;

[TestFixture]
class ParseLambdaTests
{
    [TestCaseSource(nameof(ParseLambdaCases))]
    public void TestParseLambda((string source, INode expected) td)
    {
        ParseTestTools.TestParsedAstNodes(td);
    }

    private static readonly (string, INode)[] ParseLambdaCases =
    [
        ("()=>(1)", new Lambda(0..7, [], new NumberLiteral(5..6, 1))),
        ("($a)=>(1)", new Lambda(0..9, [new FunctionParameter("a", null)], new NumberLiteral(7..8, 1))),
        ("($a)=>(1;2)", new Lambda(0..11, [new FunctionParameter("a", null)], new Sequence(7..10, [new NumberLiteral(7..8, 1), new NumberLiteral(9..10, 2)]))),
        ("($a,$b)=>(1)", new Lambda(0..12, [new FunctionParameter("a", null), new FunctionParameter("b", null)], new NumberLiteral(10..11, 1))),
        ("($a Number+)=>(1)", new Lambda(0..17, [new FunctionParameter("a", BuiltinTypeName.Number)], new NumberLiteral(15..16, 1))),
    ];
}