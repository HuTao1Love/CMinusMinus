namespace Interpreter.Optimizer;

public class DeadCodeOptimizer : IOptimizer
{
    public void Optimize(IList<Instruction> instructions, Dictionary<string, int> labels)
    {
        RemoveDeadCode(instructions, labels);
    }

    private static void RemoveDeadCode(IList<Instruction> instructions, Dictionary<string, int> labels)
    {
        var visited = new bool[instructions.Count];
        var stack = new Stack<int>();

        if (!labels.TryGetValue("main", out var main))
        {
            throw new InvalidOperationException("main not found in labels.");
        }

        stack.Push(main);

        while (stack.Count > 0)
        {
            var index = stack.Pop();

            if (visited[index])
            {
                continue;
            }

            visited[index] = true;
            var instruction = instructions[index];

            if (instruction.Type == VmInstructionType.Return)
            {
                continue;
            }

            if (instruction.Type == VmInstructionType.Jmp)
            {
                stack.Push(labels[instruction.Arguments[0]]);
                continue;
            }

            stack.Push(index + 1);

            if (instruction.Type is VmInstructionType.Jz or VmInstructionType.Call)
            {
                stack.Push(labels[instruction.Arguments[0]]);
            }
        }

        var optimized = new List<Instruction>();
        var deleted = new List<int>();

        for (var i = 0; i < instructions.Count; ++i)
        {
            if (visited[i])
            {
                optimized.Add(instructions[i]);
            }
            else
            {
                deleted.Add(i);
            }
        }

        ShiftLabels(labels, deleted);

        instructions.Clear();
        foreach (var instruction in optimized)
        {
            instructions.Add(instruction);
        }
    }

    private static void ShiftLabels(Dictionary<string, int> labels, IList<int> deleted)
    {
        foreach (var label in labels.Keys.ToList())
        {
            var lessThan = deleted.Count(index => index < labels[label]);
            labels[label] -= lessThan;
        }
    }
}
