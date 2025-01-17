using System.Globalization;
using System.Numerics;

namespace Compiler.Nodes;

public class IntegerNode(BigInteger value) : IVmNode
{
    private BigInteger _value = value;

    public VmNodeType GetNodeType() => VmNodeType.Integer;

    public string Value() => _value.ToString(CultureInfo.InvariantCulture);

    public void Negate() => _value = -_value;

    public IVmNode Add(IVmNode other) => new IntegerNode(_value + Cast(other)._value);

    public IVmNode Subtract(IVmNode other) => new IntegerNode(_value - Cast(other)._value);

    public IVmNode Multiply(IVmNode other) => new IntegerNode(_value * Cast(other)._value);

    public IVmNode Divide(IVmNode other) => new IntegerNode(_value / Cast(other)._value);

    public IVmNode Modulo(IVmNode other) => new IntegerNode(_value % Cast(other)._value);

    public bool LessThan(IVmNode other) => _value < Cast(other)._value;

    public bool GreaterThan(IVmNode other) => _value > Cast(other)._value;

    public bool LessThanOrEqual(IVmNode other) => _value <= Cast(other)._value;

    public bool GreaterThanOrEqual(IVmNode other) => _value >= Cast(other)._value;

    public bool NotEqual(IVmNode other) => _value != Cast(other)._value;

    public bool Equal(IVmNode other) => _value == Cast(other)._value;

    private static IntegerNode Cast(IVmNode node)
    {
        if (node is not IntegerNode casted)
            throw new InvalidOperationException("Cannot compare non-integer node with an integer node.");

        return casted;
    }
}
