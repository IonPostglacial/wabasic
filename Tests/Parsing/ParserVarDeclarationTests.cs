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
        ("let $a", new VariableDeclaration(0..6, null, Symbol.Local("a"), null)),
        ("let $a   ", new VariableDeclaration(0..6, null, Symbol.Local("a"), null)),
        ("let $a Number+", new VariableDeclaration(0..14, BuiltinTypeName.Number, Symbol.Local("a"), null)),
        ("let $a Array+<Number+>", new VariableDeclaration(
            0..22, 
            BuiltinTypeName.Generic("Array", [BuiltinTypeName.Number]), 
            Symbol.Local("a"), 
            null)),
        ("let $a := 1234", new VariableDeclaration(0..14, null, Symbol.Local("a"),
            new NumberLiteral(10..14, 1234))),
        ("let $a Number+ := 1234", new VariableDeclaration(0..22, BuiltinTypeName.Number, Symbol.Local("a"),
            new NumberLiteral(18..22, 1234))),
    ];
}