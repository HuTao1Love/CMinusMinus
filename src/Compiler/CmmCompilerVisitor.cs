using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using Antlr4.Runtime.Tree;
using Compiler.Ast;
using Compiler.Grammar;

namespace Compiler;

internal class CmmCompilerVisitor : CMinusMinusBaseVisitor<CmmObject>
{
    private static readonly CmmCompilerExpressionVisitor _expressionVisitor = new();
    private static readonly CmmCompilerStatementVisitor _statementVisitor = new(_expressionVisitor);

    public Program Compile(CMinusMinusParser.ProgramContext program) => (Program)VisitProgram(program);

    public override CmmObject VisitProgram(CMinusMinusParser.ProgramContext context)
        => new Program(context.function_declaration().Select(i => (FunctionDeclaration)i.Accept(this)).ToArray());

    public override CmmObject VisitFunction_declaration(CMinusMinusParser.Function_declarationContext context)
        => new FunctionDeclaration(
            context.ID(0).GetText(),
            context.ID().Skip(1).Select(i => i.GetText()).ToArray(),
            new Block(context.block().statement().Select(i => i.Accept(_statementVisitor)).ToArray()));
}

internal class CmmCompilerStatementVisitor(CmmCompilerExpressionVisitor expressionVisitor) : CMinusMinusBaseVisitor<Statement>
{
    protected override Statement DefaultResult => throw new InvalidOperationException();

    public override Statement VisitStatement_print(CMinusMinusParser.Statement_printContext context)
        => new PrintStatement(context.print().Accept(expressionVisitor));

    public override Statement VisitStatement_return(CMinusMinusParser.Statement_returnContext context)
        => new ReturnStatement(context.@return().Accept(expressionVisitor));

    public override Statement VisitStatement_assignment(CMinusMinusParser.Statement_assignmentContext context)
        => new AssignmentStatement(
            new Variable(
                context.assignment().var().ID().GetText(),
                context.assignment().var().expression().Select(e => e.Accept(expressionVisitor)).ToArray()),
            context.assignment().expression().Accept(expressionVisitor));

    public override Statement VisitStatement_array_init(CMinusMinusParser.Statement_array_initContext context)
        => new ArrayInitializationStatement(context.array_init().ID().GetText(), context.array_init().expression().Accept(expressionVisitor));

    public override Statement VisitStatement_function(CMinusMinusParser.Statement_functionContext context)
        => new FunctionStatement(new FunctionCall(
            context.function().ID().GetText(),
            context.function().expression().Select(e => e.Accept(expressionVisitor)).ToArray()));

    public override Statement VisitStatement_while(CMinusMinusParser.Statement_whileContext context)
        => new WhileStatement(
            context.@while().expression().Accept(expressionVisitor),
            GetBlock(context.@while().block()));

    public override Statement VisitStatement_for(CMinusMinusParser.Statement_forContext context)
        => new ForStatement(
            (AssignmentStatement)context.@for().start.Accept(this),
            context.@for().expression().Accept(expressionVisitor),
            (AssignmentStatement)context.@for().step.Accept(this),
            GetBlock(context.@for().block()));

    public override Statement VisitStatement_if(CMinusMinusParser.Statement_ifContext context)
        => new IfStatement(
            context.@if().expression().Accept(expressionVisitor),
            GetBlock(context.@if().ifblock),
            GetBlock(context.@if().elseblock));

    public override Statement VisitStatement_block(CMinusMinusParser.Statement_blockContext context)
        => new BlockStatement(GetBlock(context.block()));

    public override Statement VisitAssignment(CMinusMinusParser.AssignmentContext context)
        => new AssignmentStatement(
            new Variable(
                context.var().ID().GetText(),
                context.var().expression().Select(e => e.Accept(expressionVisitor)).ToArray()),
            context.expression().Accept(expressionVisitor));

    [return: NotNullIfNotNull(nameof(context))]
    private Block? GetBlock(CMinusMinusParser.BlockContext? context) =>
        context is null ? null : new Block(context.statement().Select(s => s.Accept(this)).ToArray());
}

internal class CmmCompilerExpressionVisitor : CMinusMinusBaseVisitor<Expression>
{
    protected override Expression DefaultResult => throw new InvalidOperationException();

    public override Expression VisitExpression_variable(CMinusMinusParser.Expression_variableContext context)
        => new VariableExpression(new Variable(
            context.var().ID().GetText(),
            context.var().expression().Select(e => e.Accept(this)).ToArray()));

    public override Expression VisitExpression_function(CMinusMinusParser.Expression_functionContext context)
        => new FunctionExpression(new FunctionCall(
            context.function().ID().GetText(),
            context.function().expression().Select(e => e.Accept(this)).ToArray()));

    public override Expression VisitExpression_value(CMinusMinusParser.Expression_valueContext context)
        => new ValueExpression(new IntValue(BigInteger.Parse(context.GetText(), CultureInfo.InvariantCulture)));

    public override Expression VisitExpression_negation(CMinusMinusParser.Expression_negationContext context)
        => new NegationExpression(context.expression().Accept(this));

    public override Expression VisitExpression_brackets(CMinusMinusParser.Expression_bracketsContext context)
        => context.expression().Accept(this);

    public override Expression VisitExpression_calc(CMinusMinusParser.Expression_calcContext context)
        => new CalculationExpression(context.lhs.Accept(this), context.@operator.Text, context.rhs.Accept(this));

    public override Expression VisitExpression_logical(CMinusMinusParser.Expression_logicalContext context)
        => new LogicalExpression(context.lhs.Accept(this), context.@operator.Text, context.rhs.Accept(this));
}
