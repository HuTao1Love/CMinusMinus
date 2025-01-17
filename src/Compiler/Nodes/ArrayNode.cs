using System.Collections;

namespace Compiler.Nodes;

public class ArrayNode(int size) : IVmNode, IList<IVmNode>
{
    private readonly List<IVmNode> _value = Enumerable.Range(0, size).Select(IVmNode (_) => new IntegerNode(0)).ToList();

    #region IVmNodeImplementation

    public VmNodeType GetNodeType() => VmNodeType.Array;

    public string Value() => $"[ {string.Join(", ", _value.Select(node => node.Value()))} ]";

    public void Negate() => throw new InvalidOperationException("Cannot perform action on array");

    public IVmNode Add(IVmNode other) => throw new InvalidOperationException("Cannot perform action on array");

    public IVmNode Subtract(IVmNode other) => throw new InvalidOperationException("Cannot perform action on array");

    public IVmNode Multiply(IVmNode other) => throw new InvalidOperationException("Cannot perform action on array");

    public IVmNode Divide(IVmNode other) => throw new InvalidOperationException("Cannot perform action on array");

    public IVmNode Modulo(IVmNode other) => throw new InvalidOperationException("Cannot perform action on array");

    public bool LessThan(IVmNode other) => throw new InvalidOperationException("Cannot perform action on array");

    public bool GreaterThan(IVmNode other) => throw new InvalidOperationException("Cannot perform action on array");

    public bool LessThanOrEqual(IVmNode other) => throw new InvalidOperationException("Cannot perform action on array");

    public bool GreaterThanOrEqual(IVmNode other) => throw new InvalidOperationException("Cannot perform action on array");

    public bool NotEqual(IVmNode other) => throw new InvalidOperationException("Cannot perform action on array");

    public bool Equal(IVmNode other) => throw new InvalidOperationException("Cannot perform action on array");

    #endregion

    #region ListImplementation

    public void Clear() => _value.Clear();

    public bool Contains(IVmNode item) => _value.Contains(item);

    public void CopyTo(IVmNode[] array, int arrayIndex) => _value.CopyTo(array, arrayIndex);

    public bool Remove(IVmNode item) => _value.Remove(item);

    public int Count => _value.Count;

    public bool IsReadOnly => ((IList<IVmNode>)_value).IsReadOnly;

    public IEnumerator<IVmNode> GetEnumerator() => _value.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => _value.GetEnumerator();

    void ICollection<IVmNode>.Add(IVmNode item) => _value.Add(item);

    public int IndexOf(IVmNode item) => _value.IndexOf(item);

    public void Insert(int index, IVmNode item) => _value.Insert(index, item);

    public void RemoveAt(int index) => _value.RemoveAt(index);

    public IVmNode this[int index]
    {
        get => _value[index];
        set => _value[index] = value;
    }

    #endregion
}

