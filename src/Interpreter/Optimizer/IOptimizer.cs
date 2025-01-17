namespace Interpreter.Optimizer;

public interface IOptimizer
{
    public void Optimize(IList<Instruction> instructions, Dictionary<string, int> marks);
}
