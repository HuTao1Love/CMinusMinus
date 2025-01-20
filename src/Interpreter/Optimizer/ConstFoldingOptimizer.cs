using System.Numerics;
using Compiler.Nodes;

namespace Interpreter.Optimizer;

public class ConstFoldingOptimizer : IOptimizer
{
    public void Optimize(IList<Instruction> instructions, Dictionary<string, int> labels)
    {
        ApplyConstantFolding(instructions, labels);
    }

    private static void ApplyConstantFolding(IList<Instruction> instructions, Dictionary<string, int> labels)
    {
        var optimized = new List<Tuple<Instruction, int>>();
        var deleted = new List<int>();
        // todo: replace with a stack
        var stack = new List<ConstantFoldingStackValue>();

        for (var i = 0; i < instructions.Count; ++i)
        {
            var instruction = instructions[i];

            switch (instruction.Type)
            {
                case VmInstructionType.Push:
                    if (instruction.Arguments.Count != 1)
                    {
                        throw new InvalidOperationException("push should have exactly one argument");
                    }

                    // todo: duplicated parsing code for ints
                    var arg = instruction.Arguments[0];
                    stack.Add(BigInteger.TryParse(arg, out var parsed)
                        ? new ConstantFoldingStackValue(new IntegerNode(parsed), true)
                        : new ConstantFoldingStackValue(new IntegerNode(0), false));

                    break;

                case VmInstructionType.Add:
                case VmInstructionType.Sub:
                case VmInstructionType.Mul:
                case VmInstructionType.Div:
                case VmInstructionType.Mod:
                    if (stack.Count < 2)
                    {
                        throw new InvalidOperationException("value stack does not contain 2 variables for operation");
                    }

                    var rhs = stack.Last();
                    stack.RemoveAt(stack.Count - 1);

                    var lhs = stack.Last();
                    stack.RemoveAt(stack.Count - 1);

                    if (lhs.IsConstant && rhs.IsConstant)
                    {
                        // todo: duplicated code
                        var result = instruction.Type switch
                        {
                            VmInstructionType.Add => lhs.Value.Add(rhs.Value),
                            VmInstructionType.Sub => lhs.Value.Subtract(rhs.Value),
                            VmInstructionType.Mul => lhs.Value.Multiply(rhs.Value),
                            VmInstructionType.Div => lhs.Value.Divide(rhs.Value),
                            VmInstructionType.Mod => lhs.Value.Modulo(rhs.Value),
                            _ => throw new InvalidOperationException("Unknown operation type")
                        };

                        stack.Add(new ConstantFoldingStackValue(result, true));
                        deleted.Add(i);
                    }
                    else
                    {
                        stack.Add(new ConstantFoldingStackValue(new IntegerNode(0), false));
                    }

                    break;

                case VmInstructionType.CompEQ:
                case VmInstructionType.CompGE:
                case VmInstructionType.CompGT:
                case VmInstructionType.CompLE:
                case VmInstructionType.CompLT:
                case VmInstructionType.CompNE:
                case VmInstructionType.BinAnd:
                case VmInstructionType.BinOr:
                    if (stack.Count < 2)
                    {
                        throw new InvalidOperationException("value stack does not contain 2 variables for operation");
                    }

                    rhs = stack.Last();
                    stack.RemoveAt(stack.Count - 1);

                    lhs = stack.Last();
                    stack.RemoveAt(stack.Count - 1);

                    if (lhs.IsConstant && rhs.IsConstant)
                    {
                        // todo: duplicated code
                        var result = instruction.Type switch
                        {
                            VmInstructionType.CompEQ => lhs.Value.Equal(rhs.Value),
                            VmInstructionType.CompGE => lhs.Value.GreaterThanOrEqual(rhs.Value),
                            VmInstructionType.CompGT => lhs.Value.GreaterThan(rhs.Value),
                            VmInstructionType.CompLE => lhs.Value.LessThanOrEqual(rhs.Value),
                            VmInstructionType.CompLT => lhs.Value.LessThan(rhs.Value),
                            VmInstructionType.CompNE => lhs.Value.NotEqual(rhs.Value),
                            VmInstructionType.BinAnd => lhs.Value.Value != "0" && rhs.Value.Value != "0",
                            VmInstructionType.BinOr => lhs.Value.Value != "0" || rhs.Value.Value != "0",
                            _ => throw new InvalidOperationException("Unknown comparison type")
                        };

                        stack.Add(new ConstantFoldingStackValue(new IntegerNode(result ? 1 : 0), true));
                        deleted.Add(i);
                    }
                    else
                    {
                        stack.Add(new ConstantFoldingStackValue(new IntegerNode(0), false));
                    }

                    break;
            }

            if (!deleted.Contains(i))
            {
                optimized.Add(Tuple.Create(instruction, i));
            }
            else
            {
                if (optimized.Count < 2) continue;

                deleted.RemoveAt(deleted.Count - 1);

                var op1 = optimized[^1];
                var op2 = optimized[^2];
                optimized.RemoveRange(optimized.Count - 2, 2);

                deleted.Add(op1.Item2);
                deleted.Add(op2.Item2);

                var append = new Instruction(VmInstructionType.Push, new List<string> { stack.Last().Value.Value });
                optimized.Add(Tuple.Create(append, i));
            }
        }

        ShiftLabels(labels, deleted);

        instructions.Clear();
        foreach (var tuple in optimized)
        {
            instructions.Add(tuple.Item1);
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

public class ConstantFoldingStackValue(IVmNode value, bool isConstant)
{
    public IVmNode Value { get; } = value;

    public bool IsConstant { get; } = isConstant;
}
