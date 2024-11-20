using DotNetComp.Compiler;
using DotNetComp.Compiler.Ast;

namespace DotNetComp.Tests.Parsing;

static class ParseTestTools
{
    public static void TestParsedAstNodes((string source, INode expected) td)
    {
        Tokenizer tokenizer = new(td.source);
        Parser parser = new(tokenizer, false);
        INode actual = parser.Next();
        Assert.Multiple(() =>
        {
            Assert.That(actual, Is.EqualTo(td.expected));
            Assert.That(parser.Errors, Has.Count.EqualTo(0));
        });
    }
}