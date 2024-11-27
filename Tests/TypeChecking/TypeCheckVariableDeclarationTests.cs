using DotNetComp.Compiler;
using DotNetComp.Compiler.Ast;
using DotNetComp.Compiler.Builtin;
using DotNetComp.Compiler.TypeChecking;
using DotNetComp.Compiler.Typing;

namespace DotNetComp.Tests.Typechecking;

[TestFixture]
public class TypeCheckVariableDeclarationTests
{
    [TestCaseSource(nameof(TypeCheckVariableDeclarationTestCases))]
    public void TestTypeCheckVariableDeclaration((VariableDeclaration VariableDeclaration, TypeInstance expected) td)
    {
        TypeChecker typeChecker = new(new BuiltInTypeRegistry(), new BuiltinSymbolTypeMapping());
        typeChecker.VisitVariableDeclaration(td.VariableDeclaration);
        Assert.That(td.VariableDeclaration.Type, Is.EqualTo(td.expected));
    }

    private static readonly (VariableDeclaration VariableDeclaration, TypeInstance expected)[] TypeCheckVariableDeclarationTestCases =
    [
        (
            new VariableDeclaration(0..0, BuiltinTypeName.Number, Symbol.BuiltIn("a"), new NumberLiteral(0..0, 1)), 
            BuiltinType.Number
        ),
        (
            new VariableDeclaration(0..0, BuiltinTypeName.Number with { IsNullable = true }, Symbol.BuiltIn("a"), new NumberLiteral(0..0, 1)), 
            BuiltinType.Number with { IsNullable = true }
        ),
        (
            new VariableDeclaration(0..0, null, Symbol.BuiltIn("a"), new NumberLiteral(0..0, 1)), 
            BuiltinType.Number
        ),
    ];
}