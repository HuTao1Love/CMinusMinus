namespace Compiler.Nodes;

public interface IVmNode
{
    VmNodeType GetNodeType();

    string Value();

    void Negate();

    IVmNode Add(IVmNode other);

    IVmNode Subtract(IVmNode other);

    IVmNode Multiply(IVmNode other);

    IVmNode Divide(IVmNode other);

    IVmNode Modulo(IVmNode other);

    bool LessThan(IVmNode other);

    bool GreaterThan(IVmNode other);

    bool LessThanOrEqual(IVmNode other);

    bool GreaterThanOrEqual(IVmNode other);

    bool NotEqual(IVmNode other);

    bool Equal(IVmNode other);
}
