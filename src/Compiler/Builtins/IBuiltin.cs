namespace Compiler.Builtins;

public interface IBuiltin
{
    string Name { get; }

    bool Visit(TextWriter writer, FunctionCall context);
}
