using DotNetComp.Compiler;
using DotNetComp.Compiler.Ast;
using DotNetComp.Compiler.Builtin;

namespace DotNetComp.Tests.Parsing;

[TestFixture]
public class ParserDimTests
{
    [TestCaseSource(nameof(ParseDimCases))]
    public void TestParseDim((string source, INode expected) td)
    {
        ParseTestTools.TestParsedAstNodes(td);
    }

    private static readonly (string, INode)[] ParseDimCases =
    [
        ("let $a", new VariableDeclaration(0..6, null, Symbol.Local("a"), null)),
        ("let $a   ", new VariableDeclaration(0..6, null, Symbol.Local("a"), null)),
        ("let $a Number+", new VariableDeclaration(0..14, BuiltinTypeName.Number, Symbol.Local("a"), null)),
        ("let $a As Number+", new VariableDeclaration(0..17, BuiltinTypeName.Number, Symbol.Local("a"), null)),
        ("let $a Array+<Number+>", new VariableDeclaration(
            0..22, 
            BuiltinTypeName.Generic("Array", [BuiltinTypeName.Number]), 
            Symbol.Local("a"), 
            null)),
        ("let $a As Array+<Number+>", new VariableDeclaration(
            0..25,
            BuiltinTypeName.Generic("Array", [BuiltinTypeName.Number]), 
            Symbol.Local("a"), 
            null)),
        ("let $a new Array+<Number+>", new VariableDeclaration(
            0..26,
            BuiltinTypeName.Generic("Array", [BuiltinTypeName.Number]), 
            Symbol.Local("a"), 
            new Instanciation(7..26, BuiltinTypeName.Generic("Array", [BuiltinTypeName.Number]), []))),
        ("let $a := 1234", new VariableDeclaration(0..14, null, Symbol.Local("a"),
            new NumberLiteral(10..14, 1234))),
        ("let $a Number+ := 1234", new VariableDeclaration(0..22, BuiltinTypeName.Number, Symbol.Local("a"),
            new NumberLiteral(18..22, 1234))),
        ("let $a As Number+ := 1234", new VariableDeclaration(0..25, BuiltinTypeName.Number, Symbol.Local("a"),
            new NumberLiteral(21..25, 1234))),
    ];
}