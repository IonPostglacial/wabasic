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
        ("()=>{1}", new Lambda(0..7, [], new NumberLiteral(5..6, 1))),
        ("(a)=>{1}", new Lambda(0..8, [new FunctionParameter("a", null)], new NumberLiteral(6..7, 1))),
        ("(a)=>{1;2}", new Lambda(0..10, [new FunctionParameter("a", null)], new Sequence(6..9, [new NumberLiteral(6..7, 1), new NumberLiteral(8..9, 2)]))),
        ("(a,b)=>{1}", new Lambda(0..10, [new FunctionParameter("a", null), new FunctionParameter("b", null)], new NumberLiteral(8..9, 1))),
        ("(a Number+)=>{1}", new Lambda(0..16, [new FunctionParameter("a", BuiltinTypeName.Number)], new NumberLiteral(14..15, 1))),
    ];
}