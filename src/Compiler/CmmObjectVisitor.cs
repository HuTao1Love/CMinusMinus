namespace Compiler;

public class CmmObjectVisitor(StreamWriter sw) : ICmmObjectVisitor<bool>
{
    private int _labelCounter;

    private int NextLabel() => _labelCounter++;

    public bool VisitCmmObject(CmmObject cmmObject) => cmmObject.Accept(this);

    public bool VisitValue(Value cmmObject) => cmmObject.Accept(this);

    public bool VisitIntValue(IntValue cmmObject)
    {
        sw.WriteLine($"\tpush\t{cmmObject.Value}");
        return true;
    }

    public bool VisitStatement(Statement cmmObject) => cmmObject.Accept(this);

    public bool VisitBlock(Block cmmObject) => cmmObject.Statements.All(s => s.Accept(this));

    public bool VisitVariable(Variable cmmObject)
    {
        if (cmmObject.ArrayAccess is null || cmmObject.ArrayAccess.Length == 0)
        {
            sw.Write($"\tpush\t{cmmObject.Name}");
            return true;
        }

        var result = cmmObject.ArrayAccess.All(a => a.Accept(this));

        sw.Write($"\taccess\t{cmmObject.Name}");

        return result;
    }

    public bool VisitFunctionCall(FunctionCall cmmObject)
    {
        if (BuiltinCmmFunctionDirector.TryGetBuiltin(cmmObject.Function, out var builtin))
        {
            return builtin.Visit(sw, cmmObject);
        }

        foreach (var arg in cmmObject.Args.Reverse())
        {
            VisitExpression(arg);
        }

        sw.WriteLine($"\tcall\t{cmmObject.Function}");
        return true;
    }

    public bool VisitExpression(Expression cmmObject) => cmmObject.Accept(this);

    public bool VisitVariableExpression(VariableExpression cmmObject)
    {
        var result = VisitVariable(cmmObject.Variable);
        sw.WriteLine();
        return result;
    }

    public bool VisitFunctionExpression(FunctionExpression cmmObject) => VisitFunctionCall(cmmObject.Function);

    public bool VisitValueExpression(ValueExpression cmmObject) => VisitValue(cmmObject.Value);

    public bool VisitNegationExpression(NegationExpression cmmObject)
    {
        var result = VisitExpression(cmmObject.Expression);
        sw.WriteLine("\tneg");
        return result;
    }

    public bool VisitCalculationExpression(CalculationExpression cmmObject)
    {
        var result = VisitExpression(cmmObject.Lhs) && VisitExpression(cmmObject.Rhs);
        sw.WriteLine($"\t{GetOperator(cmmObject.Operator)}");
        return result;
    }

    public bool VisitLogicalExpression(LogicalExpression cmmObject)
    {
        var result = VisitExpression(cmmObject.Lhs) && VisitExpression(cmmObject.Rhs);
        sw.WriteLine($"\t{GetOperator(cmmObject.Operator)}");
        return result;
    }

    public bool VisitPrintStatement(PrintStatement cmmObject)
    {
        var result = VisitExpression(cmmObject.Expression);
        sw.WriteLine("\tprint");
        return result;
    }

    // todo: hardcoded to always return 1 argument
    public bool VisitReturnStatement(ReturnStatement cmmObject)
    {
        var result = VisitExpression(cmmObject.Expression);
        sw.WriteLine($"\treturn\t1");
        return result;
    }

    public bool VisitAssignmentStatement(AssignmentStatement cmmObject)
    {
        var result = VisitExpression(cmmObject.Value);

        if (cmmObject.Variable.ArrayAccess is not { Length: > 0 })
        {
            sw.WriteLine($"\tpop\t{cmmObject.Variable.Name}");
            return result;
        }

        result &= cmmObject.Variable.ArrayAccess.All(index => index.Accept(this));

        sw.WriteLine($"\tpop\tarr\t{cmmObject.Variable.Name}");

        return result;
    }

    public bool VisitArrayInitializationStatement(ArrayInitializationStatement cmmObject)
    {
        var result = VisitExpression(cmmObject.Size);
        sw.WriteLine($"\tarray\t{cmmObject.Name}");
        return result;
    }

    public bool VisitFunctionStatement(FunctionStatement cmmObject) => VisitFunctionCall(cmmObject.Function);

    public bool VisitWhileStatement(WhileStatement cmmObject)
    {
        var startLabel = NextLabel();
        var endLabel = NextLabel();

        sw.WriteLine($"L{startLabel}:");
        var result = VisitExpression(cmmObject.Condition);
        sw.WriteLine($"\tjz\tL{endLabel}");
        result &= VisitBlock(cmmObject.Body);
        sw.WriteLine($"\tjmp\tL{startLabel}");
        sw.WriteLine($"L{endLabel}:");
        return result;
    }

    public bool VisitForStatement(ForStatement cmmObject)
    {
        var startLabel = NextLabel();
        var endLabel = NextLabel();

        var result = VisitAssignmentStatement(cmmObject.Start);
        sw.WriteLine($"L{startLabel}:");
        result &= VisitExpression(cmmObject.Condition);
        sw.WriteLine($"\tjz\tL{endLabel}");
        result &= VisitBlock(cmmObject.Body) && VisitAssignmentStatement(cmmObject.Step);
        sw.WriteLine($"\tjmp\tL{startLabel}");
        sw.WriteLine($"L{endLabel}:");
        return result;
    }

    public bool VisitIfStatement(IfStatement cmmObject)
    {
        var result = VisitExpression(cmmObject.Condition);

        if (cmmObject.ElseBody == null)
        {
            var label1 = NextLabel();

            sw.WriteLine($"\tjz\tL{label1}");
            result &= VisitBlock(cmmObject.Body);
            sw.WriteLine($"L{label1}:");

            return result;
        }
        else
        {
            var label1 = NextLabel();

            sw.WriteLine($"\tjz\tL{label1}");
            result &= VisitBlock(cmmObject.Body);

            var label2 = NextLabel();

            sw.WriteLine($"\tjmp\tL{label2}");
            sw.WriteLine($"L{label1}:");
            result &= VisitBlock(cmmObject.ElseBody);
            sw.WriteLine($"L{label2}:");
        }

        return result;
    }

    public bool VisitBlockStatement(BlockStatement cmmObject) => VisitBlock(cmmObject.Block);

    public bool VisitFunctionDeclaration(FunctionDeclaration cmmObject)
    {
        sw.WriteLine($"{cmmObject.Name}:");
        foreach (var arg in cmmObject.Args)
        {
            sw.WriteLine($"\tpop\t{arg}");
        }

        var result = VisitBlock(cmmObject.Body);

        if (!cmmObject.Body.Statements.Any(s => s is ReturnStatement))
        {
            sw.WriteLine("\treturn\t0");
        }

        return result;
    }

    public bool VisitProgram(Program cmmObject) => cmmObject.Functions.All(VisitFunctionDeclaration);

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
