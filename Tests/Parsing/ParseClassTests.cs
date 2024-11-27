using DotNetComp.Compiler;
using DotNetComp.Compiler.Ast;
using DotNetComp.Compiler.Builtin;

namespace DotNetComp.Tests.Parsing;

[TestFixture]
class ParseClassTests
{
    [TestCaseSource(nameof(ParseClassCases))]
    public void TestParseClass((string source, INode expected) td)
    {
        ParseTestTools.TestParsedAstNodes(td);
    }

    private static readonly (string, INode)[] ParseClassCases =
    [
        ("class Point{}", new ClassDefinition(0..12, "Point", [], null, null, [], [], [])),
        ("class Point(){}", new ClassDefinition(0..14, "Point", [], [], null, [], [], [])),
        ("class Point(x Number+, y Number+){}", 
            new ClassDefinition(0..34, "Point", [], 
                [new ("x", BuiltinTypeName.Number), new ("y", BuiltinTypeName.Number)], null, [], [], [])),
        ("class Point(){ public Distance() Any {} }", 
            new ClassDefinition(0..40, "Point", [], [], null, [], [], [
                new ClassDefinition.Method([], Accessibility.Public, "Distance", [], BuiltinTypeName.Any with { IsNullable = true }, new Sequence(40..41, []))
            ])),
        ("class Point(){ [External] public Distance() Any {} }", 
            new ClassDefinition(0..51, "Point", [], [], null, [], [], [
                new ClassDefinition.Method([new (Symbol.BuiltIn("External"), [])], 
                    Accessibility.Public, "Distance", [], BuiltinTypeName.Any with { IsNullable = true }, new Sequence(51..52, []))
            ])),
    ];
}