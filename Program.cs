using DotNetComp.Compiler;
using DotNetComp.Compiler.Ast;
using DotNetComp.Compiler.Builtin;
using DotNetComp.Compiler.JsGeneration;
using DotNetComp.Compiler.TypeChecking;

namespace DotNetComp;


class Program
{
    static void Main(string[] args)
    {
        var src = "class Point(){ [External] Public Distance() Any {} }";
        if (args.Length > 0)
        {
            src = File.ReadAllText(args[0]);
        }
        var tokenizer = new Tokenizer(src);
        var parser = new Parser(tokenizer, false);
        var typeRegistry = new BuiltInTypeRegistry();
        var typeChecker = new TypeChecker(typeRegistry, new BuiltinSymbolTypeMapping());
        var generator = new JavaScriptGenerator(typeRegistry);
        List<string> statements = [];
        while (parser.HasNext())
        {
            INode node = parser.Next();
            node.Accept(typeChecker);
            statements.Add(generator.GenerateCodeBody(node));
        }
        foreach (var parsingError in parser.Errors)
        {
            Console.WriteLine("------------------------");
            Console.WriteLine();
            Console.WriteLine(parsingError);
            Console.WriteLine(src[parsingError.Range]);
            if (parsingError is ParsingError.ExpectedButFound ebf)
            {
                Console.WriteLine("expected:");
                foreach (var tt in ebf.Kinds)
                {
                    Console.Write(tt);
                    Console.WriteLine();
                }
            }
        }
        foreach (var typeError in typeChecker.Errors)
        {
            Console.WriteLine("------------------------");
            Console.WriteLine();
            Console.WriteLine(typeError);
            Console.WriteLine(src[typeError.Range]);
        }
        Console.Write($"{generator.GenerateCodePrelude()}\n{string.Join(";\n", statements)}\n");
    }
}
