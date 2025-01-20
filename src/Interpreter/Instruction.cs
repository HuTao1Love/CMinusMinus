namespace Interpreter;

public record Instruction(VmInstructionType Type, IReadOnlyList<string> Arguments)
{
    public static Instruction FromString(string instruction)
    {
        var parts = instruction.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);

        return new Instruction(Enum.Parse<VmInstructionType>(parts[0], true), parts.Skip(1).ToList());
    }
}
