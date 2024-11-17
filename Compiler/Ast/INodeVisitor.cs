namespace DotNetComp.Compiler.Ast;

public interface INodeVisitor<T>
{
    T VisitAggregateCall(AggregateCall node);
    T VisitAssignment(Assignment node);
    T VisitBinaryOperation(BinaryOperation node);
    T VisitClassDefinition(ClassDefinition node);
    T VisitConversion(Conversion node);
    T VisitVariableDeclaration(VariableDeclaration node);
    T VisitForLoop(ForLoop node);
    T VisitForEachLoop(ForEachLoop node);
    T VisitFunctionCall(FunctionCall node);
    T VisitIfStatement(IfStatement node);
    T VisitCaseStatement(CaseStatement node);
    T VisitInvalidNode(InvalidNode node);
    T VisitLambda(Lambda node);
    T VisitListLiteral(ListLiteral node);
    T VisitMethodCall(MethodCall node);
    T VisitInstanciation(Instanciation node);
    T VisitNegation(Negation node);
    T VisitNumberLiteral(NumberLiteral node);
    T VisitReturnStatement(ReturnStatement node);
    T VisitBreakStatement(BreakStatement node);
    T VisitContinueStatement(ContinueStatement node);
    T VisitSelect(SelectCall node);
    T VisitSequence(Sequence node);
    T VisitStringLiteral(StringLiteral node);
    T VisitUnaryMinus(UnaryMinus node);
    T VisitVariable(Variable node);
    T VisitWhileLoop(WhileLoop node);
}