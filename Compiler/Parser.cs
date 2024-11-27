namespace DotNetComp.Compiler;

using Ast;

public class Parser(Tokenizer tokens, bool stopOnFirstError)
{
    private static readonly int unaryOpsBp = 21;
    public readonly List<ParsingError.IError> Errors = [];
    private readonly bool stopOnFirstError = stopOnFirstError;
    private readonly Tokenizer Tokens = tokens;

    public INode Next()
    {
        if (stopOnFirstError && Errors.Count > 0)
            return new InvalidNode(Tokens.Peek().Range);
        return ParseNextNode(0);
    }

    private INode ParseNextNode(int minBp)
    {
        INode? node = null;
        int start = Tokens.Peek().Range.Start.Value;
        do
        {
            node = ParseNextToken(node, minBp);
        } while (HasNext() && (node == null || node is InvalidNode));
        while (Tokens.Peek() is Token nextTok && node is not null)
        {
            if (nextTok.Kind == TokenKind.Sep)
                break;
            else if (nextTok.Kind == TokenKind.Operator)
            {
                node = ParseNextToken(node, minBp);
                break;
            }
            else if (nextTok.Kind == TokenKind.OpenParens)
                if (node is Variable varNode && IsConversionSym(varNode.Symbol))
                {
                    node = ParseConversion(varNode);
                }
                else
                {
                    var (args, end) = ParseSequenceList(TokenKind.CloseParens);
                    var span = new Range(start, end);
                    bool ok = true;
                    switch (node)
                    {
                        case Variable(_, Symbol(SymKind.Builtin, "", var name)):
                            if (name.Equals("return", StringComparison.OrdinalIgnoreCase))
                                (node, ok) = ReturnStatement.FromArgList(span, args);
                            else if (name.Equals("if", StringComparison.OrdinalIgnoreCase))
                                (node, ok) = IfStatement.FromArgList(span, args);
                            else if (name.Equals("case", StringComparison.OrdinalIgnoreCase))
                                (node, ok) = CaseStatement.FromArgList(span, args);
                            else if (name.Equals("for", StringComparison.OrdinalIgnoreCase))
                                (node, ok) = ForLoop.FromArgList(span, args);
                            else if (name.Equals("foreach", StringComparison.OrdinalIgnoreCase))
                                (node, ok) = ForEachLoop.FromArgList(span, args);
                            else if (name.Equals("while", StringComparison.OrdinalIgnoreCase))
                                (node, ok) = WhileLoop.FromArgList(span, args);
                            else
                                node = new FunctionCall(span, node, args);
                            if (!ok && node is ICallableStatement callable)
                                RaiseError(new ParsingError.WrongArgumentsCount(span, args, callable.Arity()));
                            break;
                        default:
                            node = new FunctionCall(span, node, args);
                            break;
                    }
                }
            else if (nextTok.Kind == TokenKind.OpenSquare)
            {
                var (args, end) = ParseSequenceList(TokenKind.CloseSquare);
                node = new SelectCall(new(start, end), node, args);
            }
            else if (nextTok.Kind == TokenKind.Dot)
            {
                Tokens.Next();
                Token member = ExpectNonNull(Tokens.Next(), [TokenKind.Sym]);
                if (member.Kind != TokenKind.Sym)
                    RaiseError(new ParsingError.ExpectedButFound([TokenKind.Sym], member));
                Token nextTok2 = ExpectNonNull(Tokens.Peek(), [TokenKind.OpenParens]);
                if (nextTok2.Kind == TokenKind.OpenParens)
                {
                    var (args, end) = ParseSequenceList(TokenKind.CloseParens);
                    node = new MethodCall(new(node.Range.Start, end), node, Tokens.AsSymbol(member), args);
                }
                else if (nextTok2.Kind == TokenKind.OpenBracket)
                {
                    var filter = ParseNextNode(0);
                    var end = Tokens.Peek().Range.End;
                    node = new AggregateCall(new(start, end), node, Tokens.AsSymbol(member), filter);
                }
                else
                {
                    RaiseError(new ParsingError.ExpectedButFound([TokenKind.OpenParens, TokenKind.OpenBracket], nextTok2));
                }
            }
            else if (nextTok.Kind == TokenKind.Assign)
            {
                Tokens.Next();
                node = new Assignment(node, AssertNode(ParseNextNode(0), "right part of :="));
                break;
            }
            else break;
        }
        return node ?? new InvalidNode(Tokens.Peek().Range);
    }

    private INode ParseNextToken(INode? previous, int minBp)
    {
        Token tok = Tokens.Peek();
        if (tok.Kind is TokenKind.Eof)
            return new InvalidNode(tok.Range);
        if (tok.Kind != TokenKind.Operator)
            Tokens.Next();
        return tok.Kind switch
        {
            TokenKind.Str => new StringLiteral(tok.Range, Tokens.AsString(tok)),
            TokenKind.Num => new NumberLiteral(tok.Range, Tokens.AsNumber(tok)),
            TokenKind.Sym => new Variable(tok.Range, Symbol.FromText(Tokens.AsSpan(tok))),
            TokenKind.Not => ParseNextNode(unaryOpsBp) switch
            {
                INode node => new Negation(tok.Range.Start..node.Range.End, node),
                _ => new InvalidNode(tok.Range),
            },
            TokenKind.New => ParseNew(tok),
            TokenKind.Let => ParseLet(tok),
            TokenKind.Operator => ParseOperation(previous, tok, minBp),
            TokenKind.OpenParens => ParseParens(tok),
            TokenKind.OpenSquare => ParseClassWithAttribute(),
            TokenKind.Class => ParseClass(tok, []),
            _ => new InvalidNode(tok.Range)
        };
    }

    private ClassDefinition ParseClassWithAttribute()
    {
        List<MetaAttribute> attributes = [];
        var firstAttribute = ParseAttribute();
        if (firstAttribute is not null)
        {
            attributes.Add(firstAttribute);
        }
        while (Tokens.Peek().Kind == TokenKind.OpenSquare)
        {
            Tokens.Next();
            MetaAttribute? attribute = ParseAttribute();
            if (attribute is not null)
            {
                attributes.Add(attribute);
            }
        }
        return ParseClass(Tokens.Next(), attributes);
    }

    private MetaAttribute? ParseAttribute()
    {
        var node = ParseNextNode(0);
        var nextTok = Tokens.Next();
        if (nextTok.Kind != TokenKind.CloseSquare)
            RaiseError(new ParsingError.ExpectedButFound([TokenKind.CloseSquare], nextTok));
        if (node is Variable variable)
        {
            return new MetaAttribute(variable.Symbol, []);
        } else if (node is FunctionCall call  && call.Callee is Variable funcName)
        {
            return new MetaAttribute(funcName.Symbol, call.Args);
        }
        else
        {   
            RaiseError(new ParsingError.UnexpectedExpression(node));
            return null;
        }
    }

    private ClassDefinition ParseClass(Token tok, List<MetaAttribute> attributes)
    {
        Token className = ExpectNonNull(Tokens.Next(), [TokenKind.Sym]);
        if (className.Kind is not TokenKind.Sym)
            RaiseError(new ParsingError.ExpectedButFound([TokenKind.Sym], className));
        var name = Tokens.AsSpan(className).ToString();
        ClassDefinition.Inheritance? inheritance = null;
        List<FunctionParameter>? constructorParams = null;
        if (Tokens.Peek().Kind == TokenKind.OpenParens)
        {
            constructorParams = ParseParamsList();
        }
        if (Tokens.Peek().Kind == TokenKind.Sep)
        {
            Tokens.Next();
            var node = ParseNextNode(0);
            if (node is Variable baseClass)
            {
                inheritance = new ClassDefinition.Inheritance(baseClass.Symbol, null);
            } 
            else if (node is FunctionCall call && call.Callee is Variable callee)
            {
                inheritance = new ClassDefinition.Inheritance(callee.Symbol, call.Args);
            }
            else
            {
                RaiseError(new ParsingError.UnexpectedExpression(node));
            }
        }
        List<ClassDefinition.Constructor> constructors = [];
        List<ClassDefinition.Property> properties = [];
        List<ClassDefinition.Method> methods = [];
        Token openBracket = ExpectNonNull(Tokens.Next(), [TokenKind.OpenBracket]);
        if (openBracket.Kind is not TokenKind.OpenBracket)
            RaiseError(new ParsingError.ExpectedButFound([TokenKind.OpenBracket], openBracket));
        while (true)
        {
            Token nextToken = ExpectNonNull(Tokens.Peek(), [TokenKind.Sym, TokenKind.Sep, TokenKind.CloseBracket]);
            if (nextToken.Kind is TokenKind.Sep)
            {
                Tokens.Next();
                continue;    
            }
            if (nextToken.Kind is TokenKind.CloseBracket or TokenKind.Eof)
            {
                Tokens.Next();
                break;
            }
            if (nextToken.Kind is not (TokenKind.Sym or TokenKind.OpenSquare))
            {
                RaiseError(new ParsingError.ExpectedButFound([TokenKind.Sym, TokenKind.CloseBracket], nextToken));
            }
            ParseClassMember(constructors, properties, methods);
        }
        return new ClassDefinition(tok.Range.Start..Tokens.Peek().Range.End, name, attributes, constructorParams, inheritance, constructors, properties, methods);
    }

    private int FindNextTokenInKeywords(string[] keywords)
    {
        Token token = ExpectNonNull(Tokens.Next(), [TokenKind.Sym]);
        if (token.Kind != TokenKind.Sym)
        {
            RaiseError(new ParsingError.ExpectedKeywordButFound(keywords, token));
            return -1;
        }
        var tokenText = Tokens.AsSpan(token);
        for (int i = 0; i < keywords.Length; i++)
        {
            if (tokenText.Equals(keywords[i], StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }
        RaiseError(new ParsingError.ExpectedKeywordButFound(keywords, token));
        return -1;
    }

    private void ParseClassMember(List<ClassDefinition.Constructor> Constructors, List<ClassDefinition.Property> Properties, List<ClassDefinition.Method> Methods)
    {
        Token firstToken = Tokens.Peek();
        List<MetaAttribute> attributes = [];
        while (Tokens.Peek().Kind == TokenKind.OpenSquare)
        {
            Tokens.Next();
            MetaAttribute? attribute = ParseAttribute();
            if (attribute is not null)
            {
                attributes.Add(attribute);
            }
        }
        int start = firstToken.Range.Start.Value;
        int accessibilityIndex = FindNextTokenInKeywords(["private", "public"]);
        Accessibility accessibility = Accessibility.Private;
        Token memberNameTok;
        if (accessibilityIndex == 1)
            accessibility = Accessibility.Public;
        if (accessibilityIndex == -1)
            memberNameTok = firstToken;
        else
            memberNameTok = ExpectNonNull(Tokens.Next(), [TokenKind.Sym]);
        ReadOnlySpan<char> memberName = Tokens.AsSpan(memberNameTok);
        if (memberName.Equals("new", StringComparison.OrdinalIgnoreCase))
        {
            var constructor = ParseConstructor(start, attributes, accessibility);
            if (constructor is not null)
            {
                Constructors.Add(constructor);
            }
        }
        else
        {
            var nextToken = Tokens.Peek();
            switch (nextToken.Kind)
            {
                case TokenKind.OpenParens:
                    var methods = ParseMethod(start, attributes, accessibility, memberName);
                    if (methods is not null)
                        Methods.Add(methods);
                    break;
                case TokenKind.Sym:
                    var prop = ParseProperty(start, attributes, accessibility, memberName);
                    if (prop is not null)
                        Properties.Add(prop);
                    break;
                default:
                    Tokens.Next();
                    RaiseError(new ParsingError.UnexpectedToken(nextToken));
                    return;
            }
        }
    }

    private ClassDefinition.Property? ParseProperty(int start, List<MetaAttribute> attributes, Accessibility accessibility, ReadOnlySpan<char> memberName)
    {
        TypeName? typeName = TryParseType();
        if (typeName is null)
        {
            RaiseError(new ParsingError.ExpectedSomeType(new Range(start, Tokens.Peek().Range.End)));
            return null;
        }
        var openBracket = Tokens.Next();
        if (openBracket.Kind != TokenKind.OpenBracket)
        {
            RaiseError(new ParsingError.ExpectedButFound([TokenKind.OpenBracket], openBracket));
            return null;
        }
        int accessibilityOrGetIndex = FindNextTokenInKeywords(["private", "public", "get"]);
        Accessibility getterAccess = Accessibility.Private;
        if (accessibilityOrGetIndex > 0)
            getterAccess = Accessibility.Public;
        else if (accessibilityOrGetIndex == -1)
            return null;
        if (accessibilityOrGetIndex < 2)
        {
            if (FindNextTokenInKeywords(["get"]) == -1)
                return null;
        }
        var sepOrBody = Tokens.Peek();
        INode? getterBody = null;
        if (sepOrBody.Kind == TokenKind.OpenBracket)
        {
            getterBody = ParseBracketedSequence();
        }
        else
        {
            Tokens.Next();
            if (sepOrBody.Kind != TokenKind.Sep)
            {
                RaiseError(new ParsingError.ExpectedButFound([TokenKind.Sep], sepOrBody));
                return null;
            }
        }
        int accessibilityOrSetIndex = FindNextTokenInKeywords(["private", "public", "set"]);
        Accessibility setterAccess = Accessibility.Private;
        if (accessibilityOrSetIndex > 0)
            setterAccess = Accessibility.Public;
        else if (accessibilityOrSetIndex == -1)
            return null;
        if (accessibilityOrSetIndex < 2)
        {
            if (FindNextTokenInKeywords(["set"]) == -1)
                return null;
        }
        // TODO: handle setter body
        var sepOrCloseBracket = Tokens.Next();
        if (sepOrCloseBracket.Kind == TokenKind.Sep)
            sepOrCloseBracket = Tokens.Next();
        if (sepOrCloseBracket.Kind != TokenKind.CloseBracket)
        {
            RaiseError(new ParsingError.ExpectedButFound([TokenKind.CloseBracket], sepOrCloseBracket));
            return null;
        }
        INode? initialValue = null;
        if (Tokens.Peek().Kind == TokenKind.Assign)
        {
            Tokens.Next();
            initialValue = ParseNextNode(0);
        }
        return new ClassDefinition.Property(
            attributes,
            accessibility,
            memberName.ToString(), 
            typeName, 
            new ClassDefinition.Getter([], getterAccess, getterBody), 
            new ClassDefinition.Setter([], setterAccess, null, null), 
            initialValue);
    }

    private ClassDefinition.Method? ParseMethod(int start, List<MetaAttribute> attributes, Accessibility accessibility, ReadOnlySpan<char> memberName)
    {
        var parameters = ParseParamsList();
        TypeName? typeName = TryParseType();
        if (typeName is null)
        {
            RaiseError(new ParsingError.ExpectedSomeType(new Range(start, Tokens.Peek().Range.End)));
            return null;
        }
        INode? body = ParseBracketedSequence();
        if (body is null)
            return null;
        return new ClassDefinition.Method(attributes, accessibility, memberName.ToString(), parameters, typeName, body);
    }

    private INode? ParseBracketedSequence()
    {
        var openBracket = Tokens.Next();
        if (openBracket.Kind != TokenKind.OpenBracket)
        {
            RaiseError(new ParsingError.ExpectedButFound([TokenKind.OpenBracket], openBracket));
            return null;
        }
        if (Tokens.Peek().Kind is TokenKind.CloseBracket)
        {
            Tokens.Next();
            return new Sequence(Tokens.Peek().Range, []);
        }
        var expression = ParseSequence();
        var closeBracket = Tokens.Next();
        if (closeBracket.Kind != TokenKind.CloseBracket)
        {
            RaiseError(new ParsingError.ExpectedButFound([TokenKind.CloseBracket], closeBracket));
            return null;
        }
        return expression;
    }

    private ClassDefinition.Constructor? ParseConstructor(int start, List<MetaAttribute> attributes, Accessibility accessibility)
    {
        var parameters = ParseParamsList();
        var nextToken = Tokens.Peek();
        ConstructorChaining? chaining = null;
        if (nextToken.Kind == TokenKind.Sep)
        {
            Tokens.Next();
            chaining = ParseConstructorChaining();
        }
        var body = ParseBracketedSequence();
        if (body is null)
            return null;
        return new ClassDefinition.Constructor(attributes, accessibility, parameters, chaining, body);
    }

    private ConstructorChaining? ParseConstructorChaining()
    {
        var node = ParseNextNode(0);
        ChainingKind kind;
        if (node is FunctionCall call && call.Callee is Variable callee && callee.Symbol.IsGlobalBuiltIn)
        {
            string name = callee.Symbol.Name;
            if (name.Equals("base", StringComparison.OrdinalIgnoreCase))
                kind = ChainingKind.Base;
            else if (name.Equals("this", StringComparison.OrdinalIgnoreCase))
                kind = ChainingKind.This;
            else
            {
                RaiseError(new ParsingError.UnexpectedExpression(node));
                return null;
            }
            return new ConstructorChaining(kind, call.Args);
        }
        else
        {
            RaiseError(new ParsingError.UnexpectedExpression(node));
            return null;
        }
    }

    private VariableDeclaration ParseLet(Token tok)
    {
        Token varToken = ExpectNonNull(Tokens.Next(), [TokenKind.Sym]);
        if (varToken.Kind != TokenKind.Sym)
            RaiseError(new ParsingError.ExpectedButFound([TokenKind.Sym], varToken));
        Symbol varSym = Tokens.AsSymbol(varToken);
        if (varSym.Kind is not SymKind.Builtin)
            RaiseError(new ParsingError.InvalidLet(varToken));
        int end = varToken.Range.End.Value;
        INode? value = null;
        TypeName? cstr;
        (cstr, int potentialEnd) = TryParseTypeWithEnd();
        if (cstr is not null)
            end = potentialEnd;
        Token nextTok = Tokens.Peek();
        if (nextTok.Kind == TokenKind.Assign)
        {
            Tokens.Next();
            value = AssertNode(ParseNextNode(0), "expression to assign");
        }
        if (value is not null)
            end = value.Range.End.Value;
        return new VariableDeclaration(new Range(tok.Range.Start, end), cstr, varSym, value);
    }

    private INode ParseNew(Token tok)
    {
        TypeName cstr;
        int end = tok.Range.End.Value;
        if (TryParseType() is TypeName t)
            cstr = t;
        else
            return new InvalidNode(tok.Range);
        var nextTok = Tokens.Peek();
        List<INode> args = [];
        if (nextTok.Kind == TokenKind.OpenParens)
        {
            (args, end) = ParseSequenceList(TokenKind.CloseParens);
        }
        return new Instanciation(new(tok.Range.Start, end), cstr, args);
    }

    private TypeName? TryParseType()
    {
        var (type, _) = TryParseTypeWithEnd();
        return type;
    }

    private (TypeName?, int) TryParseTypeWithEnd()
    {
        Token typeSym = Tokens.Peek();
        int end = typeSym.Range.End.Value;
        if (typeSym.Kind != TokenKind.Sym)
            return (null, end);

        Tokens.Next();
        Symbol sym = Tokens.AsSymbol(typeSym);
        CheckTypeKind(typeSym.Range, sym.Kind);

        List<TypeName> parameters = [];
        bool nullable = true;

        Token nextTok = Tokens.Peek();
        if (nextTok.Kind == TokenKind.Operator)
        {
            BinaryOperator op = Tokens.TokenToBinaryOperator(nextTok);
            if (op == BinaryOperator.Add)
            {
                nullable = false;
                end = nextTok.Range.End.Value;
                Tokens.Next();
                nextTok = Tokens.Peek();
                if (nextTok.Kind == TokenKind.Operator)
                {
                    op = Tokens.TokenToBinaryOperator(nextTok);
                }
            }
            if (op == BinaryOperator.Lt)
            {
                Token tok = Tokens.Next();
                while (tok.Kind != TokenKind.Eof)
                {
                    TypeName? param = TryParseType();
                    if (param != null)
                        parameters.Add(param);
                    else if (tok.Kind == TokenKind.Operator && Tokens.TokenToBinaryOperator(tok) == BinaryOperator.Gt)
                    {
                        end = tok.Range.End.Value;
                        Tokens.Next();
                        break;
                    }
                    else if (tok.Kind == TokenKind.Comma)
                    {
                        Tokens.Next();
                        continue;
                    }
                    else
                    {
                        RaiseError(new ParsingError.ExpectedButFound([TokenKind.Operator, TokenKind.Comma], tok));
                    }
                    tok = Tokens.Peek();
                }
            }
        }
        return (new TypeName(sym, parameters, nullable), end);
    }

    private void CheckTypeKind(Range span, SymKind kind)
    {
        if (kind is not (SymKind.Builtin or SymKind.Constant))
            RaiseError(new ParsingError.InvalidTypeKind(span, kind));
    }

    private INode ParseParens(Token tok)
    {
        int start = tok.Range.Start.Value;
        Token nextTok = Tokens.Peek();
        if (nextTok.Kind is TokenKind.Eof)
            RaiseError(new ParsingError.UnclosedParens(tok));
        else if (nextTok is Token { Kind: TokenKind.CloseParens })
        {
            Tokens.Next();
            nextTok = ExpectNonNull(Tokens.Peek(), [TokenKind.FatArrow]);
            if (nextTok.Kind == TokenKind.FatArrow)
                return ParseLambda(start, []);
            else
                RaiseError(new ParsingError.ExpectedButFound([TokenKind.FatArrow], nextTok));
        }

        INode firstNode = AssertNode(ParseNextNode(0), "in parens");
        nextTok = ExpectNonNull(Tokens.Peek(), []);
        if (nextTok.Kind == TokenKind.Eof)
        {
            RaiseError(new ParsingError.UnclosedParens(tok));
        }
        else if (firstNode is Variable)
        {
            TypeName? firstType = TryParseType();
            return ParseParamsListAfterFirstExpression(start, firstNode, firstType);
        }
        else if (nextTok.Kind is TokenKind.Comma)
        {
            return ParseList(tok.Range.Start.Value, firstNode);
        }
        else if (nextTok.Kind == TokenKind.CloseParens)
        {
            Tokens.Next();
            nextTok = Tokens.Peek();
            if (nextTok.Kind == TokenKind.FatArrow)
            {
                return ParseLambda(start, ListToParamsList([firstNode]));
            }
            else
            {
                return firstNode;
            }
        }
        else
        {
            RaiseError(new ParsingError.ExpectedButFound([TokenKind.Sym, TokenKind.Comma, TokenKind.CloseParens], nextTok));
        }
        return new InvalidNode(nextTok.Range);
    }

    private List<FunctionParameter> ListToParamsList(IEnumerable<INode> list)
    {
        return list.Select(node => new FunctionParameter(ExpressionToParamName(node), null)).ToList();
    }

    private string ExpressionToParamName(INode node)
    {
        if (node is Variable varNode && varNode.Symbol.IsGlobalBuiltIn)
        {
            return varNode.Symbol.Name;
        }
        else
        {
            RaiseError(new ParsingError.InvalidParameterName(node));
            return "";
        }
    }

    private ListLiteral ParseList(int start, INode firstExpr)
    {
        int end = start;
        List<INode> expressions = [firstExpr];
        Token token;
        while ((token = Tokens.Next()).Kind != TokenKind.Eof)
        {
            if (token.Kind == TokenKind.CloseParens)
            {
                end = token.Range.End.Value;
                break;
            }
            else if (token.Kind == TokenKind.Comma)
                expressions.Add(AssertNode(ParseNextNode(0), "list element"));
            else
                RaiseError(new ParsingError.ExpectedButFound([TokenKind.CloseParens, TokenKind.Comma], token));
        }
        return new ListLiteral(new Range(start, end), expressions);
    }

    private List<FunctionParameter> ParseParamsList()
    {
        List<FunctionParameter> parameters = [];
        Token token;
        while ((token = Tokens.Next()).Kind != TokenKind.Eof)
        {
            if (token.Kind is TokenKind.CloseParens)
            {
                break;
            }
            else if (token.Kind is TokenKind.Comma or TokenKind.OpenParens)
            {
                if (Tokens.Peek().Kind is TokenKind.CloseParens)
                {
                    Tokens.Next();
                    break;
                }
                string name = ExpressionToParamName(AssertNode(ParseNextNode(0), "param name"));
                TypeName? type = TryParseType();
                parameters.Add(new FunctionParameter(name, type));
            }
            else
            {
                RaiseError(new ParsingError.ExpectedButFound([TokenKind.CloseParens, TokenKind.Comma], token));
            }
        }
        return parameters;
    }

    private Lambda ParseParamsListAfterFirstExpression(int start, INode firstExpr, TypeName? firstType)
    {
        List<FunctionParameter> parameters = [new FunctionParameter(ExpressionToParamName(firstExpr), firstType)];
        parameters.AddRange(ParseParamsList());
        return ParseLambda(start, parameters);
    }

    private Lambda ParseLambda(int start, List<FunctionParameter> parameters)
    {
        Tokens.Next();
        var openingParens = ExpectNonNull(Tokens.Next(), [TokenKind.OpenBracket]);
        if (openingParens.Kind != TokenKind.OpenBracket)
        {
            RaiseError(new ParsingError.ExpectedButFound([TokenKind.OpenBracket], openingParens));
        }
        var body = ParseSequence();
        Token closingParens = Tokens.Next();
        if (closingParens.Kind == TokenKind.Eof)
        {
            RaiseError(new ParsingError.UnclosedParens(openingParens));
        }
        if (closingParens.Kind != TokenKind.CloseBracket)
        {
            RaiseError(new ParsingError.ExpectedButFound([TokenKind.CloseBracket], closingParens));
        }
        return new Lambda(start..closingParens.Range.End, parameters, body);
    }

    private INode ParseSequence()
    {
        List<INode> nodes = [];
        Token tok;
        while (true)
        {
            nodes.Add(AssertNode(ParseNextNode(0), "expr in seq"));
            tok = Tokens.Peek();
            if (tok.Kind != TokenKind.Sep)
                break;
            Tokens.Next();
            tok = Tokens.Peek();
            if (tok.Kind is TokenKind.Eof || Tokens.TokenIsSeparator(tok))
            {
                break;
            }
        }
        return NodeListToNode(nodes);
    }

    private static INode NodeListToNode(List<INode> nodes)
    {
        if (nodes.Count == 1)
            return nodes[0];
        else
            return new Sequence(new (nodes.First().Range.Start, nodes.Last().Range.End), nodes);
    }

    private Token ExpectNonNull(Token tok, TokenKind[] kinds)
    {
        if (tok.Kind is TokenKind.Eof)
            RaiseError(new ParsingError.ExpectedSome(tok.Range, kinds));
        return tok;
    }

    private INode AssertNode(INode node, string context)
    {
        if (node is InvalidNode)
        {
            RaiseError(new ParsingError.AssertExpressionFailed(node.Range, context));
        }
        return node;
    }

    public bool HasNext()
    {
        return Tokens.Peek().Kind != TokenKind.Eof;
    }

    private INode ParseBinaryOperation(INode lhs, int minBp)
    {
        while (true)
        {
            Token tok = Tokens.Peek();
            if (tok.Kind != TokenKind.Operator)
                break;
            BinaryOperator op = Tokens.TokenToBinaryOperator(tok);
            (int lbp, int rbp) = op.BindingPower();
            if (lbp < minBp)
                break;
            Tokens.Next();
            INode? nextNode = ParseNextNode(rbp);
            if (nextNode is INode rhs)
                lhs = new BinaryOperation(op, lhs, rhs);
            else
                RaiseError(new ParsingError.UnexpectedEndAfter(tok));
        }
        return lhs;
    }

    private INode ParseOperation(INode? previous, Token tok, int minBp)
    {
        if (previous is not null)
        {
            return ParseBinaryOperation(previous, minBp);
        }
        else
        {
            if (Tokens.TokenToBinaryOperator(tok) == BinaryOperator.Sub)
            {
                Tokens.Next();
                if (ParseNextNode(unaryOpsBp) is INode node)
                    return new UnaryMinus(new(tok.Range.Start, node.Range.End), node);
                else
                    RaiseError(new ParsingError.UnexpectedEndAfter(tok));
            }
            else
            {
                RaiseError(new ParsingError.UnexpectedToken(tok));
            }
        }
        return new InvalidNode(tok.Range);
    }

    private INode ParseConversion(Variable node)
    {
        string operation = node.Symbol.Name;
        Token nextToken = ExpectNonNull(Tokens.Next(), [TokenKind.OpenParens]);
        if (nextToken.Kind != TokenKind.OpenParens)
            RaiseError(new ParsingError.ExpectedButFound([TokenKind.OpenParens], nextToken));
        INode converted = AssertNode(ParseNextNode(0), "expression to convert");
        Token comma = ExpectNonNull(Tokens.Next(), [TokenKind.Comma]);
        if (comma.Kind != TokenKind.Comma)
            RaiseError(new ParsingError.ExpectedButFound([TokenKind.Comma], comma));
        TypeName? type = TryParseType();
        Token closingParens = ExpectNonNull(Tokens.Next(), [TokenKind.CloseParens]);
        if (closingParens.Kind != TokenKind.CloseParens)
            RaiseError(new ParsingError.ExpectedButFound([TokenKind.CloseParens], closingParens));
        Conversion.Operation op = ParseConversionOperation(operation, node.Range);
        if (type is not null)
            return new Conversion(node.Range, op, converted, type);
        return new InvalidNode(node.Range);
    }

    private Conversion.Operation ParseConversionOperation(string operation, Range span)
    {
        if (operation.Equals("cast", StringComparison.OrdinalIgnoreCase))
            return Conversion.Operation.Cast;
        else if (operation.Equals("trycast", StringComparison.OrdinalIgnoreCase))
            return Conversion.Operation.TryCast;
        else if (operation.Equals("deserializefromjson", StringComparison.OrdinalIgnoreCase))
            return Conversion.Operation.DeserializeFromJson;
        else
        {
            RaiseError(new ParsingError.InvalidOperation(span, operation));
            return Conversion.Operation.Cast;
        }
    }

    private (List<INode>, int) ParseSequenceList(TokenKind endTokenKind)
    {
        List<INode> sequences = [];
        Token tok = Tokens.Next();
        int end = tok.Range.End.Value;
        tok = Tokens.Peek();
        if (tok.Kind != endTokenKind)
            sequences.Add(ParseSequence());
        while (tok.Kind != TokenKind.Eof)
        {
            if (tok.Kind == endTokenKind)
            {
                end = tok.Range.End.Value;
                Tokens.Next();
                break;
            }
            else if (tok.Kind == TokenKind.Comma)
            {
                Tokens.Next();
                sequences.Add(ParseSequence());
            }
            tok = Tokens.Peek();
        }
        return (sequences, end);
    }

    private Token RaiseError(ParsingError.IError error)
    {
        Errors.Add(error);
        return new Token(TokenKind.Err, error.Range);
    }

    private static bool IsConversionSym(Symbol sym)
    {
        return sym.IsGlobalBuiltIn && Enum.GetNames(typeof(Conversion.Operation)).Contains(sym.Name, StringComparer.OrdinalIgnoreCase);
    }
}