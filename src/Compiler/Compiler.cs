using Antlr4.Runtime;
using Compiler.Grammar;
using Newtonsoft.Json;

namespace Compiler;

public static class Compiler
{
    private static CmmCompilerVisitor _compilerVisitor = new();

    public static void Compile(string file)
    {
        Console.WriteLine("HEllo");
        var compiled = $"{file}.cmmbin";
        var text = File.ReadAllText(file);
        var tokens = new AntlrInputStream(text);
        CMinusMinusLexer lexer = new(tokens);
        CommonTokenStream stream = new(lexer);
        CMinusMinusParser parser = new(stream);

        // TODO interpret as bytecode & write to file in "compiled" variable
        var program = _compilerVisitor.Compile(parser.program());
        File.WriteAllText(compiled, JsonConvert.SerializeObject(program, Formatting.Indented));
    }
}
