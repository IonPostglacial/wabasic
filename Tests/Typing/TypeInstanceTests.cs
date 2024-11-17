using DotNetComp.Compiler.Builtin;
using DotNetComp.Compiler.Builtin.Collections;
using DotNetComp.Compiler.Typing;

namespace DotNetComp.Tests.Typing;

[TestFixture]
public class TypeInstanceTests
{
    [TestCaseSource(nameof(IsSubtypeTestCases))]
    public void TestIsSubtype((TypeInstance type1, TypeInstance type2, bool expected) td)
    {
        bool actual = td.type1.IsSubtype(td.type2);
        Assert.That(actual, Is.EqualTo(td.expected));
    }

    private static readonly (TypeInstance, TypeInstance, bool)[] IsSubtypeTestCases =
    [
        (BuiltinType.Object, BuiltinType.Object, true),
        (BuiltinType.Number, BuiltinType.Object, true),
        (BuiltinType.String, BuiltinType.Object, true),
        (BuiltinType.Date, BuiltinType.Object, true),
        (BuiltinType.Object, BuiltinType.Number, false),

        (BuiltinType.Object with { IsNullable = true }, BuiltinType.Object, false),
        (BuiltinType.Object, BuiltinType.Object with { IsNullable = true }, true),
        (BuiltinType.Number, BuiltinType.Object with { IsNullable = true }, true),
        (BuiltinType.Number with { IsNullable = true }, BuiltinType.Object, false),
        (
            BuiltinCache.Of(BuiltinType.Number, BuiltinType.Number),
            BuiltinDictionary.Of(BuiltinType.Number, BuiltinType.Number),
            true
        ),
        (
            BuiltinCache.Of(BuiltinType.Number, BuiltinType.Object),
            BuiltinDictionary.Of(BuiltinType.Number, BuiltinType.Number),
            false
        ),
    ];

    [TestCaseSource(nameof(LastCommonAncestorTestCases))]
    public void TestLastCommonAncestor((TypeInstance type1, TypeInstance type2, TypeInstance expected) td)
    {
        TypeInstance actual = td.type1.LastCommonAncestor(td.type2);
        Assert.That(actual, Is.EqualTo(td.expected));
    }

    private static readonly (TypeInstance, TypeInstance, TypeInstance)[] LastCommonAncestorTestCases =
    [
        (BuiltinType.Object, BuiltinType.Object, BuiltinType.Object),
        (BuiltinType.Number, BuiltinType.Object, BuiltinType.Object),
        (BuiltinType.String, BuiltinType.String, BuiltinType.String),
        (BuiltinType.String, BuiltinType.Number, BuiltinType.Object),

        (BuiltinType.Object with { IsNullable = true }, BuiltinType.Object, BuiltinType.Object with { IsNullable = true }),
        (BuiltinType.String, BuiltinType.Number with { IsNullable = true }, BuiltinType.Object with { IsNullable = true }),

        (
            BuiltinCache.Of(BuiltinType.Number, BuiltinType.Number),
            BuiltinDictionary.Of(BuiltinType.Number, BuiltinType.Number),
            BuiltinDictionary.Of(BuiltinType.Number, BuiltinType.Number)
        ),
        (
            BuiltinCache.Of(BuiltinType.Number, BuiltinType.Number, nullable: true),
            BuiltinDictionary.Of(BuiltinType.Number, BuiltinType.Number),
            BuiltinDictionary.Of(BuiltinType.Number, BuiltinType.Number, nullable: true)
        ),
        (
            BuiltinCache.Of(BuiltinType.Number, BuiltinType.Object),
            BuiltinDictionary.Of(BuiltinType.Number, BuiltinType.Number),
            BuiltinType.Object
        ),
    ];

    [TestCaseSource(nameof(GetParentTypeTestCases))]
    public void TestGetParentType((TypeInstance type, TypeInstance expected) td)
    {
        TypeInstance actual = td.type.GetParentType();
        Assert.That(actual, Is.EqualTo(td.expected));
    }

    private static readonly (TypeInstance, TypeInstance)[] GetParentTypeTestCases =
    [
        (BuiltinType.Object, BuiltinType.Object),
        (BuiltinType.Object with { IsNullable = true }, BuiltinType.Object with { IsNullable = true }),
        (BuiltinType.Number, BuiltinType.Object),
        (BuiltinType.Number with { IsNullable = true }, BuiltinType.Object with { IsNullable = true }),

        (BuiltinArray.Of(BuiltinType.Number), BuiltinType.Object),
        (BuiltinDictionary.Of(BuiltinType.Number, BuiltinType.Number), BuiltinType.Object),
        (
            BuiltinCache.Of(BuiltinType.Number, BuiltinType.Number),
            BuiltinDictionary.Of(BuiltinType.Number, BuiltinType.Number)
        ),
    ];

    [TestCaseSource(nameof(GetMethodTypeTestCases))]
    public void TestGetMethodType((TypeInstance type, string method, int argCount, TypeInstance expected) td)
    {
        TypeInstance? actual = td.type.GetMemberType(td.method, td.argCount);
        Assert.That(actual, Is.EqualTo(td.expected));
    }

    private static readonly (TypeInstance, string, int, TypeInstance)[] GetMethodTypeTestCases =
    [
        (BuiltinArray.Of(BuiltinType.Number), "Size", 0, BuiltInTypeDef.Callable.Instanciate([BuiltinType.Number])),
        (BuiltinArray.Of(BuiltinType.Number), "Get", 1, BuiltInTypeDef.Callable.Instanciate([BuiltinType.Number, BuiltinType.Number])),
        (BuiltinArray.Of(BuiltinType.String), "Get", 1, BuiltInTypeDef.Callable.Instanciate([BuiltinType.Number, BuiltinType.String])),
        (BuiltinArray.Of(BuiltinType.Date), "New", 1, BuiltInTypeDef.Callable.Instanciate([BuiltinType.Date, BuiltinArray.Of(BuiltinType.Date)])),
        (BuiltinArray.Of(BuiltinType.Date), "New", 2, BuiltInTypeDef.Callable.Instanciate([BuiltinType.Date, BuiltinType.Date, BuiltinArray.Of(BuiltinType.Date)])),
        (BuiltinArray.Of(BuiltinType.Date), "New", 5, BuiltInTypeDef.Callable.Instanciate([
            BuiltinType.Date, BuiltinType.Date, BuiltinType.Date, BuiltinType.Date, BuiltinType.Date, BuiltinArray.Of(BuiltinType.Date)
        ])),
    ];
}