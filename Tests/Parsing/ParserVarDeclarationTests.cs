using DotNetComp.Compiler;
using DotNetComp.Compiler.Ast;
using DotNetComp.Compiler.Builtin;

namespace DotNetComp.Tests.Parsing;

[TestFixture]
public class ParserVarDeclarationTests
{
    [TestCaseSource(nameof(ParseDimCases))]
    public void TestParseLet((string source, INode expected) td)
    {
        ParseTestTools.TestParsedAstNodes(td);
    }

    private static readonly (string, INode)[] ParseDimCases =
    [
        ("let a", new VariableDeclaration(0..5, null, Symbol.BuiltIn("a"), null)),
        ("let a   ", new VariableDeclaration(0..5, null, Symbol.BuiltIn("a"), null)),
        ("let a Number+", new VariableDeclaration(0..13, BuiltinTypeName.Number, Symbol.BuiltIn("a"), null)),
        ("let a Array+<Number+>", new VariableDeclaration(
            0..21, 
            BuiltinTypeName.Generic("Array", [BuiltinTypeName.Number]), 
            Symbol.BuiltIn("a"), 
            null)),
        ("let a := 1234", new VariableDeclaration(0..13, null, Symbol.BuiltIn("a"),
            new NumberLiteral(9..13, 1234))),
        ("let a Number+ := 1234", new VariableDeclaration(0..21, BuiltinTypeName.Number, Symbol.BuiltIn("a"),
            new NumberLiteral(17..21, 1234))),
    ];
}