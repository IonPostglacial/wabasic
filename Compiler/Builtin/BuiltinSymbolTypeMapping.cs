using DotNetComp.Compiler.Builtin.Collections;
using DotNetComp.Compiler.TypeChecking;
using DotNetComp.Compiler.Typing;

namespace DotNetComp.Compiler.Builtin;

class BuiltinSymbolTypeMapping : ISymbolTypeMapping
{
    public ISymbolTypeMapping? Parent => null;
    private readonly Dictionary<Symbol, TypeInstance> typeBySymbol = [];

    public BuiltinSymbolTypeMapping()
    {
        DefStandardVariable("Null", BuiltinType.Object);
        DefStandardVariable("True", BuiltinType.Boolean);
        DefStandardVariable("False", BuiltinType.Boolean);

        DefStandardContext("Inc", BuiltinType.Number);
        DefStandardContext("InBrowser", BuiltinType.Boolean);

        DefStandardCallable("Abs", [BuiltinType.Number, BuiltinType.Number]);
        DefStandardCallable("Log", [BuiltinType.Number, BuiltinType.Number]);
        DefStandardCallable("Exp", [BuiltinType.Number, BuiltinType.Number]);
        DefStandardCallable("Ceil", [BuiltinType.Number, BuiltinType.Number]);
        DefStandardCallable("Int", [BuiltinType.Number, BuiltinType.Number]);
        DefStandardCallable("Round", [BuiltinType.Number, BuiltinType.Number]);
        DefStandardCallable("Rnd", [BuiltinType.Number]);
        DefStandardCallable("Mod", [BuiltinType.Number, BuiltinType.Number]);
        DefStandardCallable("Sqr", [BuiltinType.Number, BuiltinType.Number]);
        DefStandardCallable("Cos", [BuiltinType.Number, BuiltinType.Number]);
        DefStandardCallable("Sin", [BuiltinType.Number, BuiltinType.Number]);
        DefStandardCallable("Tan", [BuiltinType.Number, BuiltinType.Number]);
        DefStandardCallable("Atn", [BuiltinType.Number, BuiltinType.Number]);
        DefStandardCallable("NumComp", [BuiltinType.Number, BuiltinType.Number]);

        DefStandardCallable("MaxOf", [BuiltinType.Number, BuiltinType.Number]);
        DefStandardCallable("MinOf", [BuiltinType.Number, BuiltinType.Number]);

        DefStandardCallable("StringIsNullOrEmpty", [BuiltinType.String, BuiltinType.Boolean]);
        DefStandardCallable("IsNull", [BuiltinType.Object, BuiltinType.Boolean]);
        DefStandardCallable("IsSame", [BuiltinType.Object, BuiltinType.Object]);
        DefStandardCallable("DebugPrint", [BuiltinType.Object, BuiltinType.Object]);
        DefStandardCallable("Now", [BuiltinType.Date]);
        DefStandardCallable("NowUtc", [BuiltinType.Date]);

        TypeInstance arrayOfNumber = BuiltinArray.Of(BuiltinType.Number);
        DefStandardCallable("Average", [arrayOfNumber, BuiltinType.Number]);
        DefStandardCallable("Variance", [arrayOfNumber, BuiltinType.Number]);
        DefStandardCallable("Covariance", [arrayOfNumber, arrayOfNumber, BuiltinType.Number]);

        DefStandardCallable("NewBuffer", [BuiltinType.Buffer]);
    }

    public TypeInstance? Lookup(Symbol symbol)
    {
        typeBySymbol.TryGetValue(symbol, out var typeInstance);
        return typeInstance;
    }

        private void DefStandardVariable(string name, TypeInstance typeInstance)
    {
        typeBySymbol.Add(Symbol.BuiltIn(name), typeInstance);
    }

    private void DefStandardContext(string name, TypeInstance typeInstance)
    {
        typeBySymbol.Add(Symbol.Context(name), typeInstance);
    }

    private void DefStandardCallable(string name, List<TypeInstance> paramTypes)
    {
        typeBySymbol.Add(Symbol.BuiltIn(name), BuiltInTypeDef.Callable.Instanciate(paramTypes));
    }
}