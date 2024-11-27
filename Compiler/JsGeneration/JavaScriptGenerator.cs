using DotNetComp.Compiler.Ast;
using DotNetComp.Compiler.TypeChecking;
using DotNetComp.Compiler.Typing;

namespace DotNetComp.Compiler.JsGeneration;

public class JavaScriptGenerator(ITypeRegistry typeRegistry) : INodeVisitor<IJavaScriptStatement>
{
    private readonly AutomaticVariableName variableNamer = new();
    private readonly ContextVariable incVariable = new ("__inc__");
    private readonly ContextVariable keyVariable = new ("__key__");
    private readonly ContextVariable valVariable = new ("__val__");

    public string GenerateCodePrelude()
    {
        return variableNamer.GetVariableDeclarations();
    }

    public string GenerateCodeBody(INode node)
    {
        var code = node.Accept(this);
        return code.ToCode();
    }

    public IJavaScriptStatement VisitAggregateCall(AggregateCall node)
    {
        throw new NotImplementedException();
    }

    public IJavaScriptStatement VisitAssignment(Assignment node)
    {
        return new JavaScriptBinaryOperation(
            JavaScriptBinaryOperator.Assign, 
            node.Left.Accept(this), 
            node.Right.Accept(this));
    }

    public IJavaScriptStatement VisitBinaryOperation(BinaryOperation node)
    {
        return node.Operator switch
        {
            BinaryOperator.Eq => new JavaScriptBinaryOperation(
                JavaScriptBinaryOperator.Eq, 
                node.Left.Accept(this), 
                node.Right.Accept(this)),
            BinaryOperator.Ne => new JavaScriptBinaryOperation(
                JavaScriptBinaryOperator.Ne, 
                node.Left.Accept(this), 
                node.Right.Accept(this)),
            BinaryOperator.Cat => new JavaScriptBinaryOperation(
                JavaScriptBinaryOperator.Add, 
                node.Left.Accept(this), 
                node.Right.Accept(this)),
            BinaryOperator.Add => new JavaScriptBinaryOperation(
                JavaScriptBinaryOperator.Add, 
                node.Left.Accept(this),
                node.Right.Accept(this)),
            BinaryOperator.Sub => new JavaScriptBinaryOperation(
                JavaScriptBinaryOperator.Sub, 
                node.Left.Accept(this), 
                node.Right.Accept(this)),
            BinaryOperator.Mul => new JavaScriptBinaryOperation(
                JavaScriptBinaryOperator.Mul, 
                node.Left.Accept(this), 
                node.Right.Accept(this)),
            BinaryOperator.Pow => new JavaScriptCall(
                    new JavaScriptVariableExpression("Math.pow"),
                    [node.Left.Accept(this), node.Right.Accept(this)]),
            BinaryOperator.Div => new JavaScriptBinaryOperation(
                JavaScriptBinaryOperator.Div, 
                node.Left.Accept(this), 
                node.Right.Accept(this)),
            BinaryOperator.And or BinaryOperator.AndAlso => new JavaScriptBinaryOperation(
                JavaScriptBinaryOperator.And, 
                node.Left.Accept(this), 
                node.Right.Accept(this)),
            BinaryOperator.Or or BinaryOperator.OrElse => new JavaScriptBinaryOperation(
                JavaScriptBinaryOperator.Or, 
                node.Left.Accept(this), 
                node.Right.Accept(this)),
            BinaryOperator.BitAnd =>  new JavaScriptBinaryOperation(
                JavaScriptBinaryOperator.LogAnd, 
                node.Left.Accept(this), 
                node.Right.Accept(this)),
            BinaryOperator.BitOr => new JavaScriptBinaryOperation(
                JavaScriptBinaryOperator.LogOr, 
                node.Left.Accept(this), 
                node.Right.Accept(this)),
            BinaryOperator.Gt => new JavaScriptBinaryOperation(
                JavaScriptBinaryOperator.Gt, 
                node.Left.Accept(this), 
                node.Right.Accept(this)),
            BinaryOperator.Gte => new JavaScriptBinaryOperation(
                JavaScriptBinaryOperator.Gte, 
                node.Left.Accept(this), 
                node.Right.Accept(this)),
            BinaryOperator.Lt => new JavaScriptBinaryOperation(
                JavaScriptBinaryOperator.Lt, 
                node.Left.Accept(this), 
                node.Right.Accept(this)),
            BinaryOperator.Lte => new JavaScriptBinaryOperation(
                JavaScriptBinaryOperator.Lte, 
                node.Left.Accept(this), 
                node.Right.Accept(this)),
            BinaryOperator.In => new JavaScriptMethodCall(
                node.Right.Accept(this),
                "includes",
                [node.Left.Accept(this)]),
            _ => throw new NotImplementedException($"{node.Operator}"),
        };
    }

    public IJavaScriptStatement VisitCaseStatement(CaseStatement node)
    {
        return new JavaScriptSwitchBreak(
            node.Compared.Accept(this),
            node.Clauses.Select(clause => new JavaScriptSwitchClause(
                clause.Input.Accept(this), 
                clause.Result.Accept(this))).ToArray(),
            node.Default?.Accept(this));
    }

    private string ResolveNameUsingAttributes(string name, IEnumerable<MetaAttribute> attributes) {
        var className = JavaScriptVariableExpression.NormalizeName(name);
        foreach (var attr in attributes) {
            if (attr.Name.Path.Equals("js", StringComparison.InvariantCultureIgnoreCase) && 
                attr.Name.Name.Equals("name", StringComparison.InvariantCultureIgnoreCase) && attr.Arguments.Count > 0) {
                if (attr.Arguments[0] is StringLiteral(_, var value)) {
                    className = value;
                }
            }
        }
        return className;
    }

    public IJavaScriptStatement VisitClassDefinition(ClassDefinition node)
    {
        string? parent = null;
        if (node.Base is not null)
        {
            parent = new Variable(node.Range, node.Base.Parent).Accept(this).ToCode();
        }
        var constructor = node.Constructors.Count switch {
            0 when node.PrimaryConstructorParams is not null => 
                new JavaScriptClassConstructor(
                    node.PrimaryConstructorParams.Select(p =>  JavaScriptVariableExpression.NormalizeName(p.Name)), 
                    new JavaScriptStatementSequence(
                    node.Properties
                        .Where(p => p.Value is not null)
                        .Select(p => new JavaScriptBinaryOperation(
                            JavaScriptBinaryOperator.Assign,
                            new JavaScriptAccessProperty(
                                new JavaScriptVariableExpression("this"),
                                ResolveNameUsingAttributes(p.Name, p.Attributes)),
                            p.Value!.Accept(this))))),
            0 => new JavaScriptClassConstructor([], new JavaScriptEmptyStatement()),
            1 => new JavaScriptClassConstructor(node.Constructors[0].Params.Select(p => p.Name), node.Constructors[0].Body.Accept(this)),
            _ => throw new NotImplementedException(),
        };
        var properties = node.Properties.Select(p => new JavaScriptClassProperty(p.Name, p.Value?.Accept(this))).ToArray();
        var methods = node.Methods.Select(m => {
            return new JavaScriptMethod(ResolveNameUsingAttributes(m.Name, m.Attributes), m.Params.Select(p => p.Name), m.Body.Accept(this));
        });
        return new JavaScriptClass(ResolveNameUsingAttributes(node.Name, node.Attributes), parent, constructor, properties, methods);
    }

    public IJavaScriptStatement VisitConversion(Conversion node)
    {
        throw new NotImplementedException();
    }

    public IJavaScriptStatement VisitNumberLiteral(NumberLiteral node)
    {
        return new JavaScriptLiteral(node.Value);
    }

    public IJavaScriptStatement VisitStringLiteral(StringLiteral node)
    {
        return new JavaScriptLiteral(node.Value);
    }

    public IJavaScriptStatement VisitForEachLoop(ForEachLoop node)
    {
        string keyVarName = variableNamer.GenerateVariableName();
        string valVarName = variableNamer.GenerateVariableName();
        using (keyVariable.ScopedName(keyVarName))
        using (valVariable.ScopedName(valVarName))
        {
            return new JavaScriptForOfLoop(keyVarName, valVarName, node.Iterable.Accept(this), node.Body.Accept(this));
        }
    }

    public IJavaScriptStatement VisitForLoop(ForLoop node)
    {
        string iterVarName = variableNamer.GenerateLoopVariableName();
        using (incVariable.ScopedName(iterVarName))
        {
            var iterVar = new JavaScriptVariableExpression(iterVarName, true);
            var init = new JavaScriptVariableDeclaration(iterVarName, new JavaScriptLiteral(1), true);
            var condition = new JavaScriptBinaryOperation(JavaScriptBinaryOperator.Lte, iterVar, node.Number.Accept(this));
            var each = new JavaScriptPostIncrement(iterVar);
            var body = node.Body.Accept(this);
            variableNamer.FreeLoopVariable(iterVarName);
            return new JavaScriptForLoop(init, condition, each, body);
        }
    }

    public IJavaScriptStatement VisitFunctionCall(FunctionCall node)
    {
        return new JavaScriptCall(node.Callee.Accept(this), node.Args.Select(arg => arg.Accept(this)).ToArray());
    }

    private static Symbol ResolveSymbol(Symbol symbol, Range range, IEnumerable<MetaAttribute> attributes)
    {
        IEnumerable<MetaAttribute> builtIn = attributes.Where(attr => attr.Name.Name.Equals("builtin", StringComparison.OrdinalIgnoreCase));
        if (attributes.Any() && attributes.First().Arguments is [StringLiteral(_, string name)])
            return Symbol.BuiltIn(name);
        else
            return symbol;
    }

    public IJavaScriptStatement VisitInstanciation(Instanciation node)
    {
        TypeInstance? type = typeRegistry.Lookup(node.Constructor);
        IEnumerable<MetaAttribute> builtIn = type?.Definition.Attributes.Where(attr => attr.Name.Name.Equals("builtin", StringComparison.OrdinalIgnoreCase)) ?? [];
        IJavaScriptStatement constructor = new Variable(node.Range, ResolveSymbol(node.Constructor.Name, node.Range, type?.Definition.Attributes ?? [])).Accept(this);
        return new JavaScriptNewConstructor(constructor, node.Args.Select(arg => arg.Accept(this)).ToArray());
    }

    public IJavaScriptStatement VisitIfStatement(IfStatement node)
    {
        return new JavaScriptIfStatement(node.Condition.Accept(this), node.Then.Accept(this), node.Else?.Accept(this));
    }

    public IJavaScriptStatement VisitInvalidNode(InvalidNode node)
    {
        return new JavaScriptEmptyStatement();
    }

    public IJavaScriptStatement VisitLambda(Lambda node)
    {
        return new JavaScriptFunction(node.Params.Select(p => p.Name), node.Body.Accept(this));
    }

    public IJavaScriptStatement VisitListLiteral(ListLiteral node)
    {
        return new JavaScriptArray(node.Elements.Select(e => e.Accept(this)).ToArray());
    }

    public IJavaScriptStatement VisitMethodCall(MethodCall node)
    {
        if (node.Method.Kind != SymKind.Builtin)
            throw new NotImplementedException();
        var methodName = ResolveNameUsingAttributes(node.Method.Name, node.This.Type?.Definition.GetMemberWithName(node.Method.Name)?.Attributes ?? []);
        return new JavaScriptMethodCall(node.This.Accept(this), methodName, node.Args.Select(arg => arg.Accept(this)).ToArray());
    }

    public IJavaScriptStatement VisitNegation(Negation node)
    {
        return new JavaScriptUnaryOperation(JavaScriptUnaryOperator.Not, node.Child.Accept(this));
    }

    public IJavaScriptStatement VisitReturnStatement(ReturnStatement node)
    {
        return new JavaScriptReturn(node.Value.Accept(this));
    }

    public IJavaScriptStatement VisitBreakStatement(BreakStatement node)
    {
        return new JavaScriptBreak();
    }

    public IJavaScriptStatement VisitContinueStatement(ContinueStatement node)
    {
        return new JavaScriptContinue();
    }

    public IJavaScriptStatement VisitSelect(SelectCall node)
    {
        throw new NotImplementedException();
    }

    public IJavaScriptStatement VisitSequence(Sequence node)
    {
        // We cannot delay execution of the iterator because of the way context variables are generated
        return new JavaScriptStatementSequence(node.Expressions.Select(ex => ex.Accept(this)).ToArray());
    }

    public IJavaScriptStatement VisitUnaryMinus(UnaryMinus node)
    {
        return new JavaScriptUnaryOperation(JavaScriptUnaryOperator.Minus, node.Child.Accept(this));
    }

    public IJavaScriptStatement VisitVariable(Variable node)
    {
        switch (node.Symbol.Kind)
        {
            case SymKind.Builtin:
                if (node.Symbol.Name.Equals("null", StringComparison.OrdinalIgnoreCase))
                    return new JavaScriptUndefined();
                else if (node.Symbol.Name.Equals("true", StringComparison.OrdinalIgnoreCase))
                    return new JavaScriptLiteral(true);
                else if (node.Symbol.Name.Equals("false", StringComparison.OrdinalIgnoreCase))
                    return new JavaScriptLiteral(false);
                else
                    return new JavaScriptVariableExpression(JavaScriptVariableExpression.NormalizeName(node.Symbol.Name));
            case SymKind.Context:
                if (node.Symbol.Name.Equals("inc", StringComparison.OrdinalIgnoreCase))
                    return new JavaScriptVariableExpression(incVariable.CurrentVariableName, true);
                else if (node.Symbol.Name.Equals("key", StringComparison.OrdinalIgnoreCase))
                    return new JavaScriptVariableExpression(keyVariable.CurrentVariableName, true);
                else if (node.Symbol.Name.Equals("val", StringComparison.OrdinalIgnoreCase))
                    return new JavaScriptVariableExpression(valVariable.CurrentVariableName, true);
                else if (node.Symbol.Name.Equals("this", StringComparison.OrdinalIgnoreCase))
                    return new JavaScriptVariableExpression("this");
                else
                    return new JavaScriptAccessProperty(
                        new JavaScriptVariableExpression("__context__"), 
                        JavaScriptVariableExpression.NormalizeName(node.Symbol.Name));
            default:
                throw new NotImplementedException();
        }
    }

    public IJavaScriptStatement VisitVariableDeclaration(VariableDeclaration node)
    {
        return new JavaScriptVariableDeclaration(node.Symbol.Name, node.Value?.Accept(this));
    }

    public IJavaScriptStatement VisitWhileLoop(WhileLoop node)
    {
        return new JavaScriptWhileLoop(node.Condition.Accept(this), node.Body.Accept(this));
    }
}