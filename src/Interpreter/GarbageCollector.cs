using Compiler.Nodes;

namespace Interpreter;

public class GarbageCollector(Stack<Frame> frames, Stack<IVmNode> valueStack)
{
    public void Collect()
    {
        var reachableObjects = new HashSet<IVmNode>();

        foreach (var value in valueStack)
        {
            Mark(value, reachableObjects);
        }

        foreach (var frame in frames)
        {
            foreach (var variable in frame.Variables.Values)
            {
                Mark(variable, reachableObjects);
            }

            foreach (var obj in frame.Objects)
            {
                Mark(obj, reachableObjects);
            }
        }

        foreach (var frame in frames)
        {
            frame.Objects.RemoveAll(obj => !reachableObjects.Contains(obj));
        }
    }

    private static void Mark(IVmNode? node, HashSet<IVmNode> reachableObjects)
    {
        if (node == null || !reachableObjects.Add(node) || node is not ArrayNode arrayNode)
            return;

        foreach (var element in arrayNode)
        {
            Mark(element, reachableObjects);
        }
    }
}

