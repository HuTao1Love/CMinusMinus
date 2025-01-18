namespace Compiler.Ast;

public interface ICmmObjectVisitor<T>
{
    T VisitValue(Value context);

    T VisitIntValue(IntValue context);

    T VisitStatement(Statement context);

    T VisitBlock(Block context);

    T VisitVariable(Variable context);

    T VisitFunctionCall(FunctionCall context);

    T VisitExpression(Expression context);

    T VisitVariableExpression(VariableExpression context);

    T VisitFunctionExpression(FunctionExpression context);

    T VisitValueExpression(ValueExpression context);

    T VisitNegationExpression(NegationExpression context);

    T VisitCalculationExpression(CalculationExpression context);

    T VisitLogicalExpression(LogicalExpression context);

    T VisitPrintStatement(PrintStatement context);

    T VisitReturnStatement(ReturnStatement context);

    T VisitAssignmentStatement(AssignmentStatement context);

    T VisitArrayInitializationStatement(ArrayInitializationStatement context);

    T VisitFunctionStatement(FunctionStatement context);

    T VisitWhileStatement(WhileStatement context);

    T VisitForStatement(ForStatement context);

    T VisitIfStatement(IfStatement context);

    T VisitBlockStatement(BlockStatement context);

    T VisitFunctionDeclaration(FunctionDeclaration context);

    T VisitProgram(Program context);
}
