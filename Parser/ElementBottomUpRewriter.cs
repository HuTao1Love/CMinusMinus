using System.Diagnostics;
using Antlr4.Runtime.Tree;
using Parser.Grammar;

namespace Parser;

public class ElementBottomUpRewriter : CmmBaseVisitor<Element>
{
    public override Element Visit(IParseTree tree) => tree.Accept(this);
    // public override Element VisitChildren(IRuleNode node);
    // public override Element VisitTerminal(ITerminalNode node);
    // public override Element VisitErrorNode(IErrorNode node);

    public override Element VisitProgram(CmmParser.ProgramContext context)
    {
        return new Program
        {
            Functions = context.function().Select(i => i.Accept(this)).Cast<Function>().ToArray()
        };
    }

    public override Element VisitFunction(CmmParser.FunctionContext context)
    {
        return new Function
        {
            Type = (ReturnType)context.function_return_type().Accept(this),
            Name = context.name.Text,
            Args = context.ID().Select(i => i.GetText()).ToArray(),
            Block = (Block)context.block().Accept(this)
        };
    }

    public override Element VisitFunction_return_some_type(CmmParser.Function_return_some_typeContext context)
        => new ReturnType.ReturnSomeType { Type = (Type)context.type().Accept(this) };

    public override Element VisitFunction_return_void(CmmParser.Function_return_voidContext context)
        => new ReturnType.ReturnVoidType();

    public override Element VisitFunction_call(CmmParser.Function_callContext context) => new FunctionCall
    {
        Function = context.ID().GetText(),
        Args = context.expression().Select(i => i.Accept(this)).Cast<Expression>().ToArray(),
    };

    public override Element VisitBlock(CmmParser.BlockContext context) => new Block
        { 
            Statements = context.statement()
                .Select(i => i.Accept(this))
                .Cast<Statement>()
                .ToArray() 
        };

    public override Element VisitStatement_block(CmmParser.Statement_blockContext context) => context.block().Accept(this);

    public override Element VisitStatement_if(CmmParser.Statement_ifContext context) => context.@if().Accept(this);

    public override Element VisitStatement_function(CmmParser.Statement_functionContext context) => new Statement.FunctionCallStatement()
    {
        FunctionCall = (FunctionCall) context.function_call().Accept(this)
    };

    public override Element VisitStatement_assignment(CmmParser.Statement_assignmentContext context) => context.assignment().Accept(this);

    public override Element VisitStatement_assignment_init(CmmParser.Statement_assignment_initContext context) => context.assignment_with_initialization().Accept(this);

    public override Element VisitStatement_init(CmmParser.Statement_initContext context) => context.initialization().Accept(this);

    public override Element VisitStatement_return(CmmParser.Statement_returnContext context) => context.@return().Accept(this);

    public override Element VisitIf(CmmParser.IfContext context) => new Statement.IfStatement
    {
        Expression = (Expression)context.expression().Accept(this),
        If = (Block)context.ifblock.Accept(this),
        Else = (Block?)context.elseblock?.Accept(this),
    };

    public override Element VisitAssignment(CmmParser.AssignmentContext context) => new Statement.AssignmentStatement
    {
        Variable = context.ID().GetText(),
        Value = (Expression)context.expression().Accept(this),
    };

    public override Element VisitAssignment_with_initialization(CmmParser.Assignment_with_initializationContext context) =>
        new Statement.InitializationStatement
        {
            Type = (Type)context.type().Accept(this),
            Variable = context.ID().GetText(),
            Value = (Expression)context.expression().Accept(this),
        };

    public override Element VisitInitialization(CmmParser.InitializationContext context) =>
        new Statement.InitializationStatement
        {
            Type = (Type)context.type().Accept(this),
            Variable = context.ID().GetText(),
        };

    public override Element VisitReturn(CmmParser.ReturnContext context) => new Statement.ReturnStatement
    {
        Value = (Expression)context.expression().Accept(this)
    };

    public override Element VisitExpression_logical(CmmParser.Expression_logicalContext context)
        => new Expression.BinaryExpression.LogicalExpression
        {
            Left = (Expression)context.expression(0).Accept(this),
            Right = (Expression)context.expression(1).Accept(this),
        };

    public override Element VisitExpression_calc(CmmParser.Expression_calcContext context)
        => new Expression.BinaryExpression.CalcExpression
        {
            Left = (Expression)context.expression(0).Accept(this),
            Right = (Expression)context.expression(1).Accept(this),
        };

    public override Element VisitExpression_function(CmmParser.Expression_functionContext context) =>
        new Expression.FunctionCallExpression()
        {
            Value = (FunctionCall)context.function_call().Accept(this)
        };

    public override Element VisitExpression_value(CmmParser.Expression_valueContext context)
        => new Expression.ValueExpression
        {
            Value = (IValue)context.value().Accept(this),
        };

    public override Element VisitExpression_negation(CmmParser.Expression_negationContext context) =>
        new Expression.NegationExpression
        {
            Expression = (Expression)context.expression().Accept(this),
        };

    public override Element VisitExpression_variable(CmmParser.Expression_variableContext context) =>
        new Expression.VariableExpression
        {
            Variable = context.ID().GetText(),
        };

    public override Element VisitExpression_brackets(CmmParser.Expression_bracketsContext context) => context.expression().Accept(this);

    public override Element VisitType(CmmParser.TypeContext context) => new Type.ValueType
    {
        Type = context.GetText() switch
        {
            "number" => Type.ValueTypeType.Number,
            "string" => Type.ValueTypeType.String,
            "bool" => Type.ValueTypeType.Bool,
            _ => throw new UnreachableException(),
        }
    };

    public override Element VisitValue(CmmParser.ValueContext context) => VisitChildren(context);

    public override Element VisitNumber(CmmParser.NumberContext context) => new Number { Value = long.Parse(context.GetText()) };

    public override Element VisitString(CmmParser.StringContext context) => new String { Value = context.GetText() };

    public override Element VisitBool(CmmParser.BoolContext context) => new Bool { Value = bool.Parse(context.BOOL().GetText().ToLowerInvariant()) };
}