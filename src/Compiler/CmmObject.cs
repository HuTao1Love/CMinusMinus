using System.Numerics;
using Capsaicin.VisitorPattern;

namespace Compiler;

[VisitorPattern]
public partial record CmmObject;

public partial record Value : CmmObject;

public sealed partial record IntValue(BigInteger Value) : Value();

public partial record Statement : CmmObject;

public sealed partial record Block(Statement[] Statements) : CmmObject;

public sealed partial record Variable(string Name, Expression[]? ArrayAccess) : CmmObject;
public sealed partial record FunctionCall(string Function, Expression[] Args) : CmmObject;

public partial record Expression : CmmObject;
public sealed partial record VariableExpression(Variable Variable) : Expression;
public sealed partial record FunctionExpression(FunctionCall Function) : Expression;
public sealed partial record ValueExpression(Value Value) : Expression;
public sealed partial record NegationExpression(Expression Expression) : Expression;
public sealed partial record CalculationExpression(Expression Lhs, string Operator, Expression Rhs) : Expression;
public sealed partial record LogicalExpression(Expression Lhs, string Operator, Expression Rhs) : Expression;

public sealed partial record PrintStatement(Expression Expression) : Statement;
public sealed partial record ReturnStatement(Expression Expression) : Statement;
public sealed partial record AssignmentStatement(Variable Variable, Expression Value) : Statement;
public sealed partial record ArrayInitializationStatement(string Name, Expression Size) : Statement;
public sealed partial record FunctionStatement(FunctionCall Function) : Statement;
public sealed partial record WhileStatement(Expression Condition, Block Body) : Statement;
public sealed partial record ForStatement(AssignmentStatement Start, Expression Condition, AssignmentStatement Step, Block Body) : Statement;
public sealed partial record IfStatement(Expression Condition, Block Body, Block? ElseBody) : Statement;
public sealed partial record BlockStatement(Block Block) : Statement;

public sealed partial record FunctionDeclaration(string Name, string[] Args, Block Body) : CmmObject;
public sealed partial record Program(FunctionDeclaration[] Functions) : CmmObject;
