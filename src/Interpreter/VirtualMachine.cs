using Compiler.Nodes;
using Interpreter.Optimizer;

namespace Interpreter;

public class VirtualMachine(IEnumerable<IOptimizer> optimizers)
{

    private readonly IEnumerable<IOptimizer> _optimizers = optimizers;
    private List<Instruction> _instructions = [];
    private Dictionary<string, int> _marks = [];
    private Stack<Frame> _frames = new();
    private Stack<IVmNode> _valueStack = new();
    private int _instractionPointer = 0;

    #region Run

    public void Run(string compiledFilePath)
    {
        ReadInstructions(compiledFilePath);

        Console.WriteLine("Read instructions");

        foreach (var optimizer in _optimizers)
        {
            optimizer.Optimize(_instructions, _marks);
        }

        foreach (var instruction in _instructions)
        {
            Console.WriteLine(instruction.Type);
        }

        Execute();
    }

    private void ReadInstructions(string compiledFilePath)
    {
        var lines = File.ReadAllLines(compiledFilePath);

        foreach (var line in lines)
        {
            if (line.Contains(':', StringComparison.InvariantCultureIgnoreCase))
            {
                var mark = line.TrimEnd(':');
                Console.WriteLine(mark);
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
        _instractionPointer = _marks["entrypoint"];
        _frames.Push(new Frame());

        Console.WriteLine();
        Console.WriteLine("VM Execution");
        while (_instractionPointer < _instructions.Count)
        {
            var instruction = _instructions[_instractionPointer];
            Console.WriteLine("Current instruction: " + instruction.Type + " IP: " + _instractionPointer);
            Console.WriteLine("Stack:");
            foreach (var value in _valueStack)
            {
                Console.WriteLine("Type: " + value.GetNodeType() + " value: " + value.Value);
            }
            Console.WriteLine("Values:");
            foreach (var frame in _frames)
            {
                foreach (var kvp in frame.Variables)
                {
                    Console.WriteLine($"Key: {kvp.Key}, Type: {kvp.Value.GetNodeType()}, Value: {kvp.Value.Value}");
                }
            }
            Console.WriteLine();

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

            _instractionPointer++;
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
            _frames.Peek().Objects.Add(new IntegerNode(intValue));
            _valueStack.Push(_frames.Peek().Objects.Last());
        }
        else
        {
            if (!_frames.Peek().Variables.TryGetValue(value, out var result))
            {
                throw new InvalidOperationException("Invalid push argument: not an integer.");
            }

            _valueStack.Push(result);
        }
    }

    private void HandlePop(Instruction instruction)
    {
        if (_valueStack.Count == 0)
            throw new InvalidOperationException("Value stack is empty.");

        if (instruction.Arguments.Count == 1)
        {
            var arg = instruction.Arguments[0];
            _frames.Peek().Variables[arg] = _valueStack.Pop();
        }
        else
        {
            var arg = instruction.Arguments[1];
            var index = _valueStack.Pop() as IntegerNode;
            var value = _valueStack.Pop() as IntegerNode;
            var array = _frames.Peek().Variables[arg] as ArrayNode;
            Console.WriteLine($"{int.Parse(index.Value)} and {int.Parse(value.Value)}");
            array[int.Parse(index.Value)] = value;
        }
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

        // _frames.Peek().ReturnAddress = target - 1;
        _instractionPointer = target - 1;
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

        _frames.Push(new Frame { ReturnAddress = _instractionPointer + 1 });
        _instractionPointer = _marks[instruction.Arguments[0]] - 1;
    }

    private void HandleReturn(Instruction instruction)
    {
        if (_frames.Count <= 1)
        {
            // throw new InvalidOperationException("No frame to return to.");
            Console.WriteLine("Execution done");
            return;
        }

        int returnValue = int.Parse(instruction.Arguments[0]);
        var prevFrame = _frames.ToArray()[_frames.Count - 2];
        var frame = _frames.Peek();

        for (int i = 0; i < returnValue; ++i)
        {
            prevFrame.Objects.Add(_valueStack.ToArray()[_valueStack.Count - 1 - i]);
        }

        _instractionPointer = _frames.Pop().ReturnAddress - 1;
    }

    private void HandleArrayCreation(Instruction instruction)
    {
        if (_valueStack.Count == 0)
            throw new InvalidOperationException("Value stack is empty, no size for creating array.");

        if (!int.TryParse(_valueStack.Pop().Value, out var size) || size < 0)
            throw new InvalidOperationException("Invalid array size.");

        var arrName = instruction.Arguments[0];
        _frames.Peek().Objects.Add(new ArrayNode(size));
        _frames.Peek().Variables[arrName] = _frames.Peek().Objects.Last();
    }

    private void HandleArrayAccess(Instruction instruction)
    {
        if (_valueStack.Count < 1)
            throw new InvalidOperationException("Not enough values on the stack for array access.");

        var indexNode = _valueStack.Pop();
        if (!int.TryParse(indexNode.Value, out var index))
            throw new InvalidOperationException("Invalid array index.");

        var arrayNode = _frames.Peek().Variables[instruction.Arguments[0]] as ArrayNode;

        _valueStack.Push(arrayNode[index]);
    }

    private void HandleArrayLength(Instruction instruction)
    {
        var arrayNode = _frames.Peek().Variables[instruction.Arguments[0]] as ArrayNode;
        _valueStack.Push(new IntegerNode(arrayNode.Count));
    }

    #endregion
}
