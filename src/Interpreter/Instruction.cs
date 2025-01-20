namespace Interpreter;

public record Instruction(VmInstructionType Type, IReadOnlyList<string> Arguments)
{
    private static readonly char[] _delimiters = [' ', '\t'];

    public static Instruction FromString(string instruction)
    {
        var parts = instruction.Split(_delimiters, StringSplitOptions.RemoveEmptyEntries);

        return new Instruction(Enum.Parse<VmInstructionType>(parts[0], true), parts.Skip(1).ToList());
    }
}
