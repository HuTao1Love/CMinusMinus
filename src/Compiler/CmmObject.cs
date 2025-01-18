using System.Linq.Expressions;
using System.Numerics;

namespace Compiler;

public record CmmObject;

public abstract record Value : CmmObject;

public sealed record IntValue(BigInteger Value) : Value();

public abstract record Statement : CmmObject;

public record Block(Statement[] Statements) : CmmObject;

public sealed record Variable(string Name, Expression[]? ArrayAccess) : CmmObject;
public sealed record FunctionCall(string Function, Expression[] Args) : CmmObject;

public abstract record Expression : CmmObject;
public sealed record VariableExpression(Variable Variable) : Expression;
public sealed record FunctionExpression(FunctionCall Function) : Expression;
public sealed record ValueExpression(Value Value) : Expression;
public sealed record NegationExpression(Expression Expression) : Expression;
public sealed record CalculationExpression(Expression Lhs, string Operator, Expression Rhs) : Expression;
public sealed record LogicalExpression(Expression Lhs, string Operator, Expression Rhs) : Expression;

public sealed record PrintStatement(Expression Expression) : Statement;
public sealed record ReturnStatement(Expression Expression) : Statement;
public sealed record AssignmentStatement(Variable Variable, Expression Value) : Statement;
public sealed record ArrayInitializationStatement(string Name, Expression Size) : Statement;
public sealed record FunctionStatement(FunctionCall Function) : Statement;
public sealed record WhileStatement(Expression Condition, Block Body) : Statement;
public sealed record ForStatement(AssignmentStatement Start, Expression Condition, AssignmentStatement Step, Block Body) : Statement;
public sealed record IfStatement(Expression Condition, Block Body, Block? ElseBody) : Statement;
public sealed record BlockStatement(Block Block) : Statement;

public sealed record FunctionDeclaration(string Name, string[] Args, Block Body) : CmmObject;
public sealed record Program(FunctionDeclaration[] Functions) : CmmObject;
