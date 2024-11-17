namespace DotNetComp.Compiler.TypeChecking;

using DotNetComp.Compiler.Ast;
using DotNetComp.Compiler.Builtin;
using DotNetComp.Compiler.Builtin.Collections;
using DotNetComp.Compiler.Typing;


public class TypeChecker(ITypeRegistry typeRegistry, ISymbolTypeMapping presetSymbols) : INodeVisitor<object?>
{
    private readonly ITypeRegistry TypeRegistry = typeRegistry;
    private StackedSymbolTypeMapping typeForSymbol = new(presetSymbols);
    public readonly List<ITypeError> Errors = [];

    private void CheckIsSubtype(INode node, TypeInstance expected)
    {
        if (node.Type is null)
        {
            Errors.Add(new UnknownTypeButExpectedError(node.Range, expected));
        }
        else
        {
            if (!node.Type.IsSubtype(expected))
            {
                Errors.Add(new ExpectedTypeButGot(node.Range, expected, node.Type));
            }
        }
    }

    private TypeInstance EnsureKnownType(INode node, TypeInstance? maybeType)
    {
        if (maybeType is null)
        {
            Errors.Add(new UnknownTypeError(node.Range));
            return BuiltinType.Object;
        }
        else
        {
            return maybeType;
        }
    }

    private void CheckIsSubOrSuperType(INode node, TypeInstance expected)
    {
        if (node.Type is null)
        {
            Errors.Add(new UnknownTypeButExpectedError(node.Range, expected));
        }
        else
        {
            if (!node.Type.IsSubtype(expected) && !expected.IsSubtype(node.Type))
            {
                Errors.Add(new ExpectedTypeInBranch(node.Range, expected, node.Type));
            }
        }
    }

    public object? VisitAggregateCall(AggregateCall node)
    {
        node.Aggregated.Accept(this);
        node.Filter.Accept(this);
        CheckIsSubtype(node.Filter, BuiltinType.Boolean);
        return null;
    }

    public object? VisitAssignment(Assignment node)
    {
        node.Left.Accept(this);
        node.Right.Accept(this);
        node.Type = node.Left.Type ?? BuiltinType.Object;
        CheckIsSubtype(node.Right, node.Type);
        return null;
    }

    public object? VisitBinaryOperation(BinaryOperation node)
    {
        node.Left.Accept(this);
        node.Right.Accept(this);
        switch (node.Operator)
        {
            case BinaryOperator.Add or BinaryOperator.Sub or BinaryOperator.Mul or BinaryOperator.Div or BinaryOperator.Pow or 
                BinaryOperator.BitAnd or BinaryOperator.BitAnd:
                CheckIsSubtype(node.Left, BuiltinType.Number);
                CheckIsSubtype(node.Right, BuiltinType.Number);
                node.Type = BuiltinType.Number;
                break;
            case BinaryOperator.Lt or BinaryOperator.Lte or BinaryOperator.Gt or BinaryOperator.Gte:
                CheckIsSubtype(node.Left, BuiltinType.Number);
                CheckIsSubtype(node.Right, BuiltinType.Number);
                node.Type = BuiltinType.Boolean;
                break;
            case BinaryOperator.And or BinaryOperator.AndAlso or BinaryOperator.Or or BinaryOperator.OrElse:
                CheckIsSubtype(node.Left, BuiltinType.Boolean);
                CheckIsSubtype(node.Right, BuiltinType.Boolean);
                node.Type = BuiltinType.Boolean;
                break;
            case BinaryOperator.Eq or BinaryOperator.Ne:
                node.Type = BuiltinType.Boolean;
                break;
            case BinaryOperator.In:
                // TODO: check left has the right interface
                node.Type = BuiltinType.Boolean;
                break;
        }
        return null;
    }

    public object? VisitClassDefinition(ClassDefinition node)
    {
        List<PropertyDefinition> properties = [];
        List<MethodDefinition> methods = [];
        var thisSymbol = Symbol.Context("this");
        TypeDefinition? parentType = null;
        if (node.Base is not null)
        {
            parentType = TypeRegistry.Lookup(node.Base.Parent);
        }
        var typeDefinition = new TypeDefinition(
            new Symbol(SymKind.Constant, "", node.Name), [],
            parentType,
            node.Attributes,
            properties,
            methods);
        TypeRegistry.Register(typeDefinition);
        WithLocalScope(classScope =>
        {
            if (node.PrimaryConstructorParams is not null)
            {
                List<ITypeParameter> constructorType = [];
                foreach (var param in node.PrimaryConstructorParams)
                {
                    var paramType = EnsureKnownType(node, TypeRegistry.Lookup(param.Type ?? BuiltinTypeName.Object));
                    constructorType.Add(new FixedTypeParameter(paramType));
                    classScope.BindSymbol(Symbol.Local(param.Name), paramType);
                }
                constructorType.Add(new ThisTypeParameters());
                methods.Add(new MethodDefinition("New", [], constructorType));
            }
            foreach (var prop in node.Properties)
            {
                var propType = EnsureKnownType(node, TypeRegistry.Lookup(prop.Type));
                var propParamType = new FixedTypeParameter(propType);
                properties.Add(new PropertyDefinition(prop.Name, propParamType));
                methods.Add(new MethodDefinition(prop.Name, prop.Attributes, [propParamType]));
                if (prop.Value is not null)
                {
                    prop.Value.Accept(this);
                    CheckIsSubtype(prop.Value, propType);
                }
            }
        });
        foreach (var constructor in node.Constructors)
        {
            WithLocalScope(constructorScope => 
            {
                constructorScope.BindSymbol(thisSymbol, typeDefinition.Instanciate(false));
                List<ITypeParameter> constructorType = [];
                foreach (var param in constructor.Params)
                {
                    var paramType = EnsureKnownType(node, TypeRegistry.Lookup(param.Type ?? BuiltinTypeName.Object));
                    constructorType.Add(new FixedTypeParameter(paramType));
                    constructorScope.BindSymbol(Symbol.Local(param.Name), paramType);
                }
                constructorType.Add(new ThisTypeParameters());
                methods.Add(new MethodDefinition("New", constructor.Attributes, constructorType));
                constructor.Body.Accept(this);
            });
        }
        foreach (var method in node.Methods)
        {
            WithLocalScope(methodScope => 
            {
                methodScope.BindSymbol(thisSymbol, typeDefinition.Instanciate(false));
                List<ITypeParameter> methodType = [];
                foreach (var param in method.Params)
                {
                    var paramType = EnsureKnownType(node, TypeRegistry.Lookup(param.Type ?? BuiltinTypeName.Object));
                    methodType.Add(new FixedTypeParameter(paramType));
                    methodScope.BindSymbol(Symbol.Local(param.Name), paramType);
                }
                var returnType = EnsureKnownType(node, TypeRegistry.Lookup(method.ReturnType));
                methodType.Add(new FixedTypeParameter(returnType));
                methods.Add(new MethodDefinition(method.Name, method.Attributes, methodType));
                method.Body.Accept(this);
            });
        }
        return null;
    }

    public object? VisitConversion(Conversion node)
    {
        node.Node.Accept(this);
        TypeInstance destinationType = EnsureKnownType(node, TypeRegistry.Lookup(node.ToType));
        CheckIsSubOrSuperType(node.Node, destinationType);
        node.Type = destinationType;
        return null;
    }

    public object? VisitForEachLoop(ForEachLoop node)
    {
        node.Iterable.Accept(this);
        // TODO: check that iterable is actually iterable<K, V>
        // TODO: add types K, V for %Key and %Val
        node.Body.Accept(this);
        node.Type = node.Body.Type;
        return null;
    }

    public object? VisitForLoop(ForLoop node)
    {
        node.Number.Accept(this);
        CheckIsSubtype(node.Number, BuiltinType.Number);
        node.Body.Accept(this);
        node.Type = node.Body.Type;
        return null;
    }

    private TypeInstance CheckCallable(Range range, TypeInstance? maybeCallable, List<INode> args)
    {
        TypeInstance returnType = BuiltinType.Object;
        if (maybeCallable is TypeInstance callable && callable.IsCallable())
        {
            int paramsCount = callable.CallableParamsCount();
            returnType = callable.CallableReturnType() ?? returnType;
            // TODO: add support for varargs
            if (paramsCount == args.Count)
            {
                foreach (var (arg, paramType) in args.Zip(callable.Arguments))
                {
                    arg.Accept(this);
                    CheckIsSubtype(arg, paramType);
                }
            }
            else
            {
                Errors.Add(new WrongArityError(range, paramsCount, args.Count));
            }
        }
        else
        {
            Errors.Add(new NotCallableTypeError(range, maybeCallable));
        }
        return returnType;
    }

    public object? VisitFunctionCall(FunctionCall node)
    {
        node.Callee.Accept(this);
        node.Type = CheckCallable(node.Range, node.Callee.Type, node.Args);
        return null;
    }

    public object? VisitIfStatement(IfStatement node)
    {
        node.Condition.Accept(this);
        node.Then.Accept(this);
        if (node.Else is not null)
        {
            node.Else.Accept(this);
            node.Type = node.Then.Type?.LastCommonAncestor(node.Else.Type ?? BuiltinType.Object);
        }
        else if (node.Then.Type is not null)
        {
            node.Type = node.Then.Type with { IsNullable = true };
        }
        return null;
    }

    public object? VisitInstanciation(Instanciation node)
    {
        foreach (var arg in node.Args)
        {
            arg.Accept(this);
        }
        node.Type = EnsureKnownType(node, TypeRegistry.Lookup(node.Constructor));
        var methodType = node.Type.GetMemberType("New", node.Args.Count);
        if (methodType is null)
        {
            Errors.Add(new UnknownMethodError(node.Range, node.Type, "New"));
            return null;
        }
        node.Type = CheckCallable(node.Range, methodType, node.Args);
        return null;
    }

    public object? VisitInvalidNode(InvalidNode node) => null;

    private void WithLocalScope(Action<StackedSymbolTypeMapping> callback)
    {
        StackedSymbolTypeMapping outerScopeSymbols = typeForSymbol;
        StackedSymbolTypeMapping localScopedSymbols = new (typeForSymbol);
        typeForSymbol = localScopedSymbols;
        callback(localScopedSymbols);
        typeForSymbol = outerScopeSymbols;
    }

    public object? VisitLambda(Lambda node)
    {
        WithLocalScope(lambdaScopedSymbols =>
        {
            List<TypeInstance> lambdaParamTypes = [];
            foreach (var param in node.Params)
            {
                var paramType = TypeRegistry.Lookup(param.Type ?? BuiltinTypeName.Object) ?? BuiltinType.Object;
                lambdaScopedSymbols.BindSymbol(Symbol.Local(param.Name), paramType);
                lambdaParamTypes.Add(paramType);
            }
            node.Body.Accept(this);
            if (node.Body.Type is null)
            {
                Errors.Add(new UnknownTypeError(node.Range));
                return;
            }
            lambdaParamTypes.Add(node.Body.Type);
            node.Type = BuiltinFunc.Definition.Instanciate(lambdaParamTypes);
        });
        return null;
    }

    public object? VisitListLiteral(ListLiteral node)
    {
        TypeInstance? common = null;
        foreach (var element in node.Elements)
        {
            element.Accept(this);
            if (common is null)
                common = element.Type;
            else
                common = common.LastCommonAncestor(element.Type ?? BuiltinType.Object);
        }
        node.Type = BuiltinArray.Of(common ?? BuiltinType.Object);
        return null;
    }

    public object? VisitMethodCall(MethodCall node)
    {
        node.This.Accept(this);
        if (node.This.Type is null)
        {
            Errors.Add(new UnknownTypeError(node.Range));
            return null;
        }
        if (node.Method.Kind != SymKind.Builtin)
        {
            throw new NotImplementedException();
        }
        var methodType = node.This.Type.GetMemberType(node.Method.Name, node.Args.Count);
        if (methodType is null)
        {
            Errors.Add(new UnknownMethodError(node.Range, node.This.Type, node.Method.Name));
            return null;
        }
        node.Type = CheckCallable(node.Range, methodType, node.Args);
        return null;
    }

    public object? VisitNegation(Negation node)
    {
        node.Child.Accept(this);
        CheckIsSubtype(node.Child, BuiltinType.Boolean);
        node.Type = BuiltinType.Boolean;
        return null;
    }

    public object? VisitNumberLiteral(NumberLiteral node)
    {
        node.Type = BuiltinType.Number;
        return null;
    }

    public object? VisitReturnStatement(ReturnStatement node)
    {
        node.Value.Accept(this);
        node.Type = node.Value.Type;
        return null;
    }

    public object? VisitSelect(SelectCall node)
    {
        node.Selected.Accept(this);
        foreach (var arg in node.Args)
        {
            arg.Accept(this);
        }
        return null;
        // TODO: check is we can select using some metadata
    }

    public object? VisitSequence(Sequence node)
    {
        foreach (var expr in node.Expressions)
        {
            expr.Accept(this);
        }
        if (node.Expressions.Count == 0)
        {
            node.Type = BuiltinType.Object;
        }
        else
        {
            node.Type = node.Expressions.Last().Type;
        }
        return null;
    }

    public object? VisitStringLiteral(StringLiteral node)
    {
        node.Type = BuiltinType.String;
        return null;
    }

    public object? VisitUnaryMinus(UnaryMinus node)
    {
        node.Child.Accept(this);
        CheckIsSubtype(node.Child, BuiltinType.Number);
        node.Type = BuiltinType.Number;
        return null;
    }

    public object? VisitVariable(Variable node)
    {
        node.Type = typeForSymbol.Lookup(node.Symbol);
        return null;
    }

    public object? VisitVariableDeclaration(VariableDeclaration node)
    {
        TypeInstance? nodeType = null;
        if (node.Value is not null)
        {
            node.Value.Accept(this);
            if (node.DeclaredType is not null)
            {
                nodeType = EnsureKnownType(node, TypeRegistry.Lookup(node.DeclaredType));
                CheckIsSubtype(node.Value, nodeType);
            }
        }
        var variableType = nodeType ?? node.Value?.Type ?? BuiltinType.Object;
        typeForSymbol.BindSymbol(node.Symbol, variableType);
        node.Type = variableType;
        return null;
    }

    public object? VisitWhileLoop(WhileLoop node)
    {
        node.Condition.Accept(this);
        CheckIsSubtype(node.Condition, BuiltinType.Boolean);
        node.Body.Accept(this);
        node.Type = node.Body.Type;
        return null;
    }

    public object? VisitCaseStatement(CaseStatement node)
    {
        TypeInstance? returnType = null;
        node.Compared.Accept(this);
        var nullableObjectType = BuiltinType.Object with { IsNullable = true };
        var comparedType = node.Compared.Type ?? nullableObjectType;
        foreach (var clause in node.Clauses)
        {
            clause.Input.Accept(this);
            CheckIsSubtype(clause.Input, comparedType);
            clause.Result.Accept(this);
            if (returnType is null)
            {
                returnType = clause.Result.Type ?? nullableObjectType;
            }
            else if (clause.Result.Type is not null)
            {
                returnType = returnType.LastCommonAncestor(clause.Result.Type);
            }
            else
            {
                returnType = nullableObjectType;
            }
        }
        node.Default?.Accept(this);
        if (returnType is not null && node.Default is not null && node.Default.Type is not null)
        {
            returnType = returnType.LastCommonAncestor(node.Default.Type);
        }
        else
        {
            returnType = nullableObjectType;
        }
        node.Type = returnType;
        return null;
    }

    public object? VisitBreakStatement(BreakStatement node)
    {
        // TODO: read papers about it
        node.Type = BuiltinType.Object;
        return null;
    }

    public object? VisitContinueStatement(ContinueStatement node)
    {
        // TODO: read papers about it
        node.Type = BuiltinType.Object;
        return null;
    }
}