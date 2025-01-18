using Antlr4.Runtime;
using Compiler.Grammar;

namespace Compiler;

public static class Compiler
{
    private static CmmCompilerVisitor _compilerVisitor = new();

    public static void Compile(string file)
    {
        var compiled = $"{file}.cmmbin";
        var text = File.ReadAllText(compiled);
        var tokens = new AntlrInputStream(text);
        CMinusMinusLexer lexer = new(tokens);
        CommonTokenStream stream = new(lexer);
        CMinusMinusParser parser = new(stream);

        // TODO interpret as bytecode & write to file
        var program = _compilerVisitor.Compile(parser.program());
    }
}
