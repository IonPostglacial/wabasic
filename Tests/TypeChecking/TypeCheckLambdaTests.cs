using DotNetComp.Compiler;
using DotNetComp.Compiler.Ast;
using DotNetComp.Compiler.Builtin;
using DotNetComp.Compiler.Builtin.Collections;
using DotNetComp.Compiler.TypeChecking;
using DotNetComp.Compiler.Typing;

namespace DotNetComp.Tests.Typechecking;

[TestFixture]
public class TypeCheckLambdaTests
{
    [TestCaseSource(nameof(TypeCheckLambdaTestCases))]
    public void TestTypeCheckLambda((Lambda lambda, TypeInstance expected) td)
    {
        TypeChecker typeChecker = new(new BuiltInTypeRegistry(), new BuiltinSymbolTypeMapping());
        typeChecker.VisitLambda(td.lambda);
        Assert.That(td.lambda.Type, Is.EqualTo(td.expected));
    }

    private static readonly (Lambda lambda, TypeInstance expected)[] TypeCheckLambdaTestCases =
    [
        (
            new Lambda(0..0, [new FunctionParameter("a", BuiltinTypeName.String)], new NumberLiteral(0..0, 1)), 
            BuiltinFunc.Definition.Instanciate([BuiltinType.String, BuiltinType.Number])
        ),
        (
            new Lambda(0..0, [new FunctionParameter("a", BuiltinTypeName.String)], new Variable(0..0, Symbol.BuiltIn("a"))), 
            BuiltinFunc.Definition.Instanciate([BuiltinType.String, BuiltinType.String])
        ),
    ];
}