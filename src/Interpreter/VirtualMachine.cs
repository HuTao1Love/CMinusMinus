using Compiler.Nodes;
using Interpreter.Optimizer;

namespace Interpreter;

public class VirtualMachine
{
    private static readonly IOptimizer[] _optimizers =
    [
        new ConstFoldingOptimizer(),
    ];

    private List<Instruction> _instructions = new();
    private Dictionary<string, int> _marks = new();
    private Stack<Frame> _frames = new();
    private Stack<IVmNode> _valueStack = new();

    #region Run

    public void Run()
    {
        ReadInstructions();

        foreach (var optimizer in _optimizers)
        {
            optimizer.Optimize(_instructions, _marks);
        }

        Execute();
    }

    private void ReadInstructions()
    {
        var lines = File.ReadAllLines("instructions.txt");

        foreach (var line in lines)
        {
            if (line.Contains(':'))
            {
                var mark = line.TrimEnd(':');
                _marks[mark] = _instructions.Count;
            }
            else
            {
                _instructions.Add(Instruction.FromString(line));
            }
        }
    }

    private void Execute()
    {
        int currentInstructionIndex = _marks["entrypoint"];
        _frames.Push(new Frame());

        while (currentInstructionIndex < _instructions.Count)
        {
            var instruction = _instructions[currentInstructionIndex];
            switch (instruction.Type)
            {
                case VmInstructionType.Push:
                    HandlePush(instruction);
                    break;
                case VmInstructionType.Pop:
                    HandlePop(instruction);
                    break;
                case VmInstructionType.Print:
                    HandlePrint();
                    break;
                case VmInstructionType.Add:
                    HandleBinaryOperation((a, b) => a.Add(b));
                    break;
                case VmInstructionType.Sub:
                    HandleBinaryOperation((a, b) => a.Subtract(b));
                    break;
                case VmInstructionType.Mul:
                    HandleBinaryOperation((a, b) => a.Multiply(b));
                    break;
                case VmInstructionType.Div:
                    HandleBinaryOperation((a, b) => a.Divide(b));
                    break;
                case VmInstructionType.Mod:
                    HandleBinaryOperation((a, b) => a.Modulo(b));
                    break;
                case VmInstructionType.CompLT:
                    HandleComparison((a, b) => a.LessThan(b));
                    break;
                case VmInstructionType.CompGT:
                    HandleComparison((a, b) => a.GreaterThan(b));
                    break;
                case VmInstructionType.CompGE:
                    HandleComparison((a, b) => a.GreaterThanOrEqual(b));
                    break;
                case VmInstructionType.CompLE:
                    HandleComparison((a, b) => a.LessThanOrEqual(b));
                    break;
                case VmInstructionType.CompNE:
                    HandleComparison((a, b) => a.NotEqual(b));
                    break;
                case VmInstructionType.CompEQ:
                    HandleComparison((a, b) => a.Equal(b));
                    break;
                case VmInstructionType.Jz:
                    HandleJumpIfZero(instruction);
                    break;
                case VmInstructionType.Jmp:
                    HandleJump(instruction);
                    break;
                case VmInstructionType.Neg:
                    HandleNegate();
                    break;
                case VmInstructionType.Call:
                    HandleCall(instruction);
                    break;
                case VmInstructionType.Return:
                    HandleReturn(instruction);
                    break;
                case VmInstructionType.Array:
                    HandleArrayCreation(instruction);
                    break;
                case VmInstructionType.Access:
                    HandleArrayAccess(instruction);
                    break;
                case VmInstructionType.Length:
                    HandleArrayLength(instruction);
                    break;
                case VmInstructionType.BinAnd:
                    HandleBinaryOperation((a, b) => new IntegerNode(a.Value != "0" && b.Value != "0" ? 1 : 0));
                    break;
                case VmInstructionType.BinOr:
                    HandleBinaryOperation((a, b) => new IntegerNode(a.Value != "0" || b.Value != "0" ? 1 : 0));
                    break;
                default:
                    throw new InvalidOperationException($"Unknown instruction type: {instruction.Type}");
            }

            currentInstructionIndex++;
        }
    }

    #endregion

    #region HandleOperations

    private void HandlePush(Instruction instruction)
    {
        if (instruction.Arguments.Count != 1)
            throw new InvalidOperationException("Push instruction requires exactly one argument.");

        var value = instruction.Arguments[0];
        if (int.TryParse(value, out int intValue))
        {
            _valueStack.Push(new IntegerNode(intValue));
        }
        else
        {
            throw new InvalidOperationException("Invalid push argument: not an integer.");
        }
    }

    private void HandlePop(Instruction instruction)
    {
        if (_valueStack.Count == 0)
            throw new InvalidOperationException("Value stack is empty.");

        _valueStack.Pop();
    }

    private void HandlePrint()
    {
        if (_valueStack.Count == 0)
            throw new InvalidOperationException("Value stack is empty.");

        Console.WriteLine(_valueStack.Peek().Value);
    }

    private void HandleBinaryOperation(Func<IVmNode, IVmNode, IVmNode> operation)
    {
        if (_valueStack.Count < 2)
            throw new InvalidOperationException("Not enough values on the stack.");

        var b = _valueStack.Pop();
        var a = _valueStack.Pop();
        _valueStack.Push(operation(a, b));
    }

    private void HandleComparison(Func<IVmNode, IVmNode, bool> comparison)
    {
        if (_valueStack.Count < 2)
            throw new InvalidOperationException("Not enough values on the stack.");

        var b = _valueStack.Pop();
        var a = _valueStack.Pop();
        _valueStack.Push(new IntegerNode(comparison(a, b) ? 1 : 0));
    }

    private void HandleJumpIfZero(Instruction instruction)
    {
        if (_valueStack.Count == 0)
            throw new InvalidOperationException("Value stack is empty.");

        var condition = _valueStack.Pop();
        if (condition.Value == "0")
        {
            HandleJump(instruction);
        }
    }

    private void HandleJump(Instruction instruction)
    {
        if (instruction.Arguments.Count != 1)
            throw new InvalidOperationException("Jump instruction requires exactly one argument.");

        if (!_marks.TryGetValue(instruction.Arguments[0], out var target))
            throw new InvalidOperationException("Invalid jump target.");

        _frames.Peek().ReturnAddress = target - 1;
    }

    private void HandleNegate()
    {
        if (_valueStack.Count == 0)
            throw new InvalidOperationException("Value stack is empty.");

        var value = _valueStack.Pop();
        value.Negate();
        _valueStack.Push(value);
    }

    private void HandleCall(Instruction instruction)
    {
        if (instruction.Arguments.Count != 1)
            throw new InvalidOperationException("Call instruction requires one argument.");

        if (!_marks.TryGetValue(instruction.Arguments[0], out var target))
            throw new InvalidOperationException("Invalid function target.");

        var frame = new Frame { ReturnAddress = _frames.Peek().ReturnAddress };
        _frames.Push(frame);
        _frames.Peek().ReturnAddress = target;
    }

    private void HandleReturn(Instruction instruction)
    {
        if (_frames.Count <= 1)
            throw new InvalidOperationException("No frame to return to.");

        var frame = _frames.Pop();
        _frames.Peek().ReturnAddress = frame.ReturnAddress;
    }

    private void HandleArrayCreation(Instruction instruction)
    {
        if (_valueStack.Count == 0)
            throw new InvalidOperationException("Value stack is empty, no size for creating array.");

        if (!int.TryParse(_valueStack.Pop().Value, out var size) || size < 0)
            throw new InvalidOperationException("Invalid array size.");

        _valueStack.Push(new ArrayNode(size));
    }

    private void HandleArrayAccess(Instruction instruction)
    {
        if (_valueStack.Count < 2)
            throw new InvalidOperationException("Not enough values on the stack for array access.");

        var indexNode = _valueStack.Pop();
        if (!int.TryParse(indexNode.Value, out var index))
            throw new InvalidOperationException("Invalid array index.");

        var arrayNode = _valueStack.Pop() as ArrayNode;
        if (arrayNode == null)
            throw new InvalidOperationException("Top of stack is not an array.");

        _valueStack.Push(arrayNode[index]);
    }

    private void HandleArrayLength(Instruction instruction)
    {
        if (_valueStack.Count == 0)
            throw new InvalidOperationException("Value stack is empty, no array for getting length.");

        var arrayNode = _valueStack.Pop() as ArrayNode;
        if (arrayNode == null)
            throw new InvalidOperationException("Top of stack is not an array.");

        _valueStack.Push(new IntegerNode(arrayNode.Count));
    }

    #endregion
}
