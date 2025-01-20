namespace Compiler.Builtins;

public class LenBuiltin : IBuiltin
{
    public string Name => "len";

    public bool Visit(TextWriter writer, FunctionCall context)
    {
        if (context.Args.Length != 1)
        {
            throw new NotSupportedException($"len only supports a single argument.");
        }

        var arg = context.Args[0];

        if (arg is VariableExpression variableExpr)
        {
            writer.WriteLine($"\tlength\t{variableExpr.Variable.Name}");
        }
        else
        {
            throw new NotSupportedException($"Unsupported expression type in len.");
        }

        return true;
    }
}
