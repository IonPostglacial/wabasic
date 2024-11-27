namespace DotNetComp.Compiler;

[TestFixture]
public class SymbolTests
{
    [SetUp]
    public void Setup()
    {
    }

    [TestCase("hello", SymKind.Builtin, "", "hello")]
    [TestCase("@hello", SymKind.Member, "", "hello")]
    [TestCase("#hello", SymKind.Constant, "", "hello")]
    [TestCase("$hello", SymKind.Context, "", "hello")]

    [TestCase("#Cst!hello", SymKind.Constant, "Cst", "hello")]
    [TestCase("#\\app\\Cst!hello", SymKind.Constant, "\\app\\Cst", "hello")]
    public void SingleToken(string symbol, SymKind kind, string path, string name)
    {
        Symbol sym = Symbol.FromText(symbol);
        Assert.Multiple(() =>
        {
            Assert.That(sym.Kind, Is.EqualTo(kind));
            Assert.That(sym.Path, Is.EqualTo(path));
            Assert.That(sym.Name, Is.EqualTo(name));
        });
    }
}