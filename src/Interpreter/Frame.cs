using Compiler.Nodes;

namespace Interpreter;

public class Frame
{
    public Dictionary<string, IVmNode> Variables { get; private set; } = new();

    public List<IVmNode> Objects { get; private set; } = new();

    public int ReturnAddress { get; set; } = -1;
}
