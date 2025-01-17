namespace Interpreter;

public class Instruction
{
    public VmInstructionType Type { get; set; }

    public List<string> Arguments { get; set; } = [];

    public static Instruction FromString(string instruction)
    {
        var parts = instruction.Split('\t');
        return new Instruction
        {
            Type = Enum.Parse<VmInstructionType>(parts[0], true),
            Arguments = parts.Skip(1).ToList()
        };
    }
}
