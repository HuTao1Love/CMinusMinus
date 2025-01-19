using Antlr4.Runtime;
using Compiler.Ast;
using Compiler.Grammar;
using Newtonsoft.Json;

namespace Compiler;

public static class Compiler
{
    private static CmmCompilerVisitor _compilerVisitor = new();

    public static void Compile(string file)
    {
        var compiled = $"{file}.cmmbin";
        var text = File.ReadAllText(file);
        var tokens = new AntlrInputStream(text);
        CMinusMinusLexer lexer = new(tokens);
        CommonTokenStream stream = new(lexer);
        CMinusMinusParser parser = new(stream);

        var program = _compilerVisitor.Compile(parser.program());

        using StreamWriter sw = new(compiled);

        CmmObjectVisitor objectVisitor = new(sw);
        objectVisitor.VisitProgram(program);
    }
}
