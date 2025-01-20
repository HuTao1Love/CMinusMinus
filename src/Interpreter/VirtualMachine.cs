using System.Globalization;
using Compiler.Nodes;
using Interpreter.Optimizer;

namespace Interpreter;

public class VirtualMachine
{
    private readonly List<Instruction> _instructions = [];
    private readonly Dictionary<string, int> _marks = [];
    private readonly Stack<Frame> _frames = new();
    private readonly Stack<IVmNode> _valueStack = new();
    private readonly GarbageCollector _gc;
    private int _instructionPointer;
    private readonly TextWriter _output;
    private readonly IEnumerable<IOptimizer> _optimizers;

    public VirtualMachine(TextWriter output, IEnumerable<IOptimizer> optimizers)
    {
        _output = output;
        _optimizers = optimizers;
        _gc = new GarbageCollector(_frames, _valueStack);
    }

    #region Run

    public void Run(string compiledFilePath)
    {
        ReadInstructions(compiledFilePath);

        _output.WriteLine("Read instructions");

        // todo optimize in runtime
        foreach (var optimizer in _optimizers)
        {
            optimizer.Optimize(_instructions, _marks);
        }

        foreach (var instruction in _instructions)
        {
            _output.WriteLine(instruction.Type);
        }

        Execute();
    }

    private void ReadInstructions(string compiledFilePath)
    {
        var lines = File.ReadAllLines(compiledFilePath);

        foreach (var line in lines)
        {
            if (!line.Contains(':', StringComparison.InvariantCultureIgnoreCase))
            {
                _instructions.Add(Instruction.FromString(line));
                continue;
            }

            var mark = line.TrimEnd(':');
            _output.WriteLine(mark);
            _marks[mark] = _instructions.Count;
        }
    }

    private void Execute()
    {
        if (!_marks.TryGetValue("main", out _instructionPointer))
        {
            throw new InvalidOperationException("main function not found");
        }

        _frames.Push(new Frame());

        _output.WriteLine();
        _output.WriteLine("VM Execution");
        while (_instructionPointer < _instructions.Count)
        {
            var instruction = _instructions[_instructionPointer];
            _output.WriteLine("Current instruction: " + instruction.Type + " IP: " + _instructionPointer);
            _output.WriteLine("Stack:");

            foreach (var value in _valueStack)
            {
                _output.WriteLine("Type: " + value.GetNodeType() + " value: " + value.Value);
            }

            _output.WriteLine("Values:");
            foreach (var kvp in _frames.SelectMany(frame => frame.Variables))
            {
                _output.WriteLine($"Key: {kvp.Key}, Type: {kvp.Value.GetNodeType()}, Value: {kvp.Value.Value}");
            }

            _output.WriteLine();

            switch (instruction.Type)
            {
                case VmInstructionType.Push:
                    CollectGarbage();
                    HandlePush(instruction);
                    break;
                case VmInstructionType.Pop:
                    HandlePop(instruction);
                    CollectGarbage();
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
                    CollectGarbage();
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

            _instructionPointer++;
        }
    }

    private int _sinceLastGarbageCollect;

    private void CollectGarbage()
    {
        if (++_sinceLastGarbageCollect != 500) return;
        _gc.Collect();
        _sinceLastGarbageCollect = 0;
    }

    #endregion

    #region HandleOperations

    private void HandlePush(Instruction instruction)
    {
        var value = instruction.Arguments.Single();
        if (int.TryParse(value, out var intValue))
        {
            _frames.Peek().Objects.Add(new IntegerNode(intValue));
            _valueStack.Push(_frames.Peek().Objects.Last());
            return;
        }

        if (!_frames.Peek().Variables.TryGetValue(value, out var result))
        {
            throw new InvalidOperationException("Invalid push argument: not an integer.");
        }

        _valueStack.Push(result);
    }

    private void HandlePop(Instruction instruction)
    {
        if (_valueStack.Count == 0) throw new InvalidOperationException("Value stack is empty.");

        if (instruction.Arguments.Count == 1)
        {
            _frames.Peek().Variables[instruction.Arguments.Single()] = _valueStack.Pop();
            return;
        }

        var arg = instruction.Arguments[1];
        var index = (IntegerNode)_valueStack.Pop();
        var value = (IntegerNode)_valueStack.Pop();
        var array = (ArrayNode)_frames.Peek().Variables[arg];
        _output.WriteLine($"{index.Value} and {value.Value}");
        array[int.Parse(index.Value, CultureInfo.InvariantCulture)] = value;
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

        if (_valueStack.Pop().Value == "0")
        {
            HandleJump(instruction);
        }
    }

    private void HandleJump(Instruction instruction)
    {
        if (!_marks.TryGetValue(instruction.Arguments.Single(), out var target))
            throw new InvalidOperationException("Invalid jump target.");

        // _frames.Peek().ReturnAddress = target - 1;
        _instructionPointer = target - 1;
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
        if (!_marks.TryGetValue(instruction.Arguments.Single(), out var target))
            throw new InvalidOperationException("Invalid function target.");

        _frames.Push(new Frame { ReturnAddress = _instructionPointer + 1 });
        _instructionPointer = _marks[instruction.Arguments[0]] - 1;
    }

    private void HandleReturn(Instruction instruction)
    {
        if (_frames.Count <= 1)
        {
            // throw new InvalidOperationException("No frame to return to.");
            _output.WriteLine("Execution done");
            return;
        }

        var returnValue = int.Parse(instruction.Arguments[0], CultureInfo.InvariantCulture);
        var prevFrame = _frames.ToArray()[_frames.Count - 2];
        var frame = _frames.Peek();

        var asArray = _valueStack.ToArray();
        for (var i = 0; i < returnValue; ++i)
        {
            prevFrame.Objects.Add(asArray[_valueStack.Count - 1 - i]);
        }

        _instructionPointer = _frames.Pop().ReturnAddress - 1;
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

        if (_frames.Peek().Variables[instruction.Arguments[0]] is not ArrayNode arrayNode) throw new InvalidOperationException();
        _valueStack.Push(arrayNode[index]);
    }

    private void HandleArrayLength(Instruction instruction)
    {
        if (_frames.Peek().Variables[instruction.Arguments[0]] is not ArrayNode arrayNode) throw new InvalidOperationException();
        _valueStack.Push(new IntegerNode(arrayNode.Count));
    }

    #endregion
}
