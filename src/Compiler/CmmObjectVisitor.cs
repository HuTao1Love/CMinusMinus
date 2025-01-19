using Compiler.Ast;

namespace Compiler;

public class CmmObjectVisitor : ICmmObjectVisitor<bool>
{
    private readonly StreamWriter _sw;
    private int _labelCounter;

    public CmmObjectVisitor(StreamWriter sw)
    {
        _sw = sw;
        _labelCounter = 0;
    }

    private int NextLabel() => _labelCounter++;

    public bool VisitValue(Value context) => context switch
    {
        IntValue intValue => VisitIntValue(intValue),
        _ => throw new NotSupportedException("Unsupported value type.")
    };

    public bool VisitIntValue(IntValue context)
    {
        _sw.WriteLine($"\tpush\t{context.Value}");
        return true;
    }

    public bool VisitStatement(Statement context) => context switch
    {
        PrintStatement print => VisitPrintStatement(print),
        ReturnStatement ret => VisitReturnStatement(ret),
        AssignmentStatement assign => VisitAssignmentStatement(assign),
        ArrayInitializationStatement arrayInit => VisitArrayInitializationStatement(arrayInit),
        FunctionStatement funcStmt => VisitFunctionStatement(funcStmt),
        WhileStatement whileStmt => VisitWhileStatement(whileStmt),
        ForStatement forStmt => VisitForStatement(forStmt),
        IfStatement ifStmt => VisitIfStatement(ifStmt),
        BlockStatement blockStmt => VisitBlockStatement(blockStmt),
        _ => throw new NotSupportedException("Unsupported statement type.")
    };

    public bool VisitBlock(Block context)
    {
        foreach (var statement in context.Statements)
        {
            VisitStatement(statement);
        }

        return true;
    }

    public bool VisitVariable(Variable context)
    {
        if (context.ArrayAccess is not null && context.ArrayAccess.Length != 0)
        {
            foreach (var expr in context.ArrayAccess)
            {
                VisitExpression(expr);
            }

            _sw.Write($"\taccess\t{context.Name}");
        }
        else
        {
            _sw.Write($"\tpush\t{context.Name}");
        }

        return true;
    }

    public bool VisitFunctionCall(FunctionCall context)
    {
        // todo: hardcoded builtin function
        if (context.Function == "len")
        {
            return VisitLength(context);
        }

        foreach (var arg in context.Args.Reverse())
        {
            VisitExpression(arg);
        }

        _sw.WriteLine($"\tcall\t{context.Function}");
        return true;
    }

    public bool VisitExpression(Expression context) => context switch
    {
        VariableExpression varExpr => VisitVariableExpression(varExpr),
        FunctionExpression funcExpr => VisitFunctionExpression(funcExpr),
        ValueExpression valExpr => VisitValueExpression(valExpr),
        NegationExpression negExpr => VisitNegationExpression(negExpr),
        CalculationExpression calcExpr => VisitCalculationExpression(calcExpr),
        LogicalExpression logExpr => VisitLogicalExpression(logExpr),
        _ => throw new NotSupportedException("Unsupported expression type.")
    };

    public bool VisitVariableExpression(VariableExpression context)
    {
        VisitVariable(context.Variable);
        _sw.WriteLine();
        return true;
    }

    public bool VisitFunctionExpression(FunctionExpression context) => VisitFunctionCall(context.Function);

    public bool VisitValueExpression(ValueExpression context) => VisitValue(context.Value);

    public bool VisitNegationExpression(NegationExpression context)
    {
        VisitExpression(context.Expression);
        _sw.WriteLine("\tneg");
        return true;
    }

    public bool VisitCalculationExpression(CalculationExpression context)
    {
        VisitExpression(context.Lhs);
        VisitExpression(context.Rhs);
        _sw.WriteLine($"\t{GetOperator(context.Operator)}");
        return true;
    }

    public bool VisitLogicalExpression(LogicalExpression context)
    {
        VisitExpression(context.Lhs);
        VisitExpression(context.Rhs);
        _sw.WriteLine($"\t{GetOperator(context.Operator)}");
        return true;
    }

    public bool VisitPrintStatement(PrintStatement context)
    {
        VisitExpression(context.Expression);
        _sw.WriteLine("\tprint");
        return true;
    }

    // todo: hardcoded to always return 1 argument
    public bool VisitReturnStatement(ReturnStatement context)
    {
        VisitExpression(context.Expression);
        _sw.WriteLine($"\treturn\t1");
        return true;
    }

    public bool VisitAssignmentStatement(AssignmentStatement context)
    {
        VisitExpression(context.Value);

        if (context.Variable.ArrayAccess != null && context.Variable.ArrayAccess.Length > 0)
        {
            foreach (var index in context.Variable.ArrayAccess)
            {
                VisitExpression(index);
            }

            _sw.WriteLine($"\tpop\tarr\t{context.Variable.Name}");
        }
        else
        {
            _sw.WriteLine($"\tpop\t{context.Variable.Name}");
        }

        return true;
    }

    public bool VisitArrayInitializationStatement(ArrayInitializationStatement context)
    {
        VisitExpression(context.Size);
        _sw.WriteLine($"\tarray\t{context.Name}");
        return true;
    }

    public bool VisitFunctionStatement(FunctionStatement context) => VisitFunctionCall(context.Function);

    public bool VisitWhileStatement(WhileStatement context)
    {
        var startLabel = NextLabel();
        var endLabel = NextLabel();

        _sw.WriteLine($"L{startLabel}:");
        VisitExpression(context.Condition);
        _sw.WriteLine($"\tjz\tL{endLabel}");
        VisitBlock(context.Body);
        _sw.WriteLine($"\tjmp\tL{startLabel}");
        _sw.WriteLine($"L{endLabel}:");
        return true;
    }

    public bool VisitForStatement(ForStatement context)
    {
        var startLabel = NextLabel();
        var endLabel = NextLabel();

        VisitAssignmentStatement(context.Start);
        _sw.WriteLine($"L{startLabel}:");
        VisitExpression(context.Condition);
        _sw.WriteLine($"\tjz\tL{endLabel}");
        VisitBlock(context.Body);
        VisitAssignmentStatement(context.Step);
        _sw.WriteLine($"\tjmp\tL{startLabel}");
        _sw.WriteLine($"L{endLabel}:");
        return true;
    }

    public bool VisitIfStatement(IfStatement context)
    {
        VisitExpression(context.Condition);

        if (context.ElseBody != null)
        {
            var label1 = NextLabel();

            _sw.WriteLine($"\tjz\tL{label1}");
            VisitBlock(context.Body);

            var label2 = NextLabel();

            _sw.WriteLine($"\tjmp\tL{label2}");
            _sw.WriteLine($"L{label1}:");
            VisitBlock(context.ElseBody);
            _sw.WriteLine($"L{label2}:");
        }
        else
        {
            var label1 = NextLabel();

            _sw.WriteLine($"\tjz\tL{label1}");
            VisitBlock(context.Body);
            _sw.WriteLine($"L{label1}:");
        }

        return true;
    }

    public bool VisitBlockStatement(BlockStatement context)
    {
        return VisitBlock(context.Block);
    }

    public bool VisitFunctionDeclaration(FunctionDeclaration context)
    {
        _sw.WriteLine($"{context.Name}:");
        foreach (var arg in context.Args)
        {
            _sw.WriteLine($"\tpop\t{arg}");
        }

        VisitBlock(context.Body);
        _sw.WriteLine("\treturn\t0");
        return true;
    }

    public bool VisitProgram(Program context)
    {
        return context.Functions.All(func => VisitFunctionDeclaration(func));
    }

    // todo: hardcoded builtin function
    public bool VisitLength(FunctionCall context)
    {
        if (context.Args.Length != 1)
        {
            throw new NotSupportedException($"len only supports a single argument.");
        }

        var arg = context.Args[0];

        if (arg is VariableExpression variableExpr)
        {
            _sw.WriteLine($"\tlength\t{variableExpr.Variable.Name}");
        }
        else
        {
            throw new NotSupportedException($"Unsupported expression type in len.");
        }

        return true;
    }

    private static string GetOperator(string op) => op switch
    {
        "+" => "add",
        "-" => "sub",
        "*" => "mul",
        "/" => "div",
        "%" => "mod",
        "<" => "compLT",
        ">" => "compGT",
        "<=" => "compLE",
        ">=" => "compGE",
        "==" => "compEQ",
        "!=" => "compNE",
        "||" => "binOR",
        "&&" => "binAND",
        _ => throw new NotSupportedException($"Unsupported operator: {op}")
    };
}
