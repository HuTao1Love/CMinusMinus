using System.Diagnostics.CodeAnalysis;
using Antlr4.Runtime;
using Parser.Grammar;

namespace Parser;

public static class Parser
{
    private static readonly ElementBottomUpRewriter Rewriter = new();

    public static Elements.Program Parse(string text)
    {
        if (!TryParse(text, out var program, out var errors))
        {
            throw new ApplicationException($"[Compilation Failed]\n" +
                                           $"Errors:\n" + string.Join("\n", errors));
        }

        return program;
    }

    public static bool TryParse(
        string text, [NotNullWhen(true)] out Elements.Program? program, [NotNullWhen(false)] out IEnumerable<string>? errors)
    {
        program = null;
        errors = null;

        var errorListener = new ErrorListener();
        TextWriter output = new StringWriter();

        var inputStream = new AntlrInputStream(text);
        var lexer = new CmmLexer(inputStream, output, output);
        lexer.AddErrorListener(errorListener);
        var cts = new CommonTokenStream(lexer);
        var parser = new CmmParser(cts, output, output);
        parser.AddErrorListener(errorListener);

        if (errorListener.Errors.Count != 0 || !string.IsNullOrEmpty(output.ToString()))
        {
            errors = errorListener.Errors.Select(e => $"[{e.Line}:{e.Position}] {e.Message}").Prepend(output.ToString()!);
            return false;
        }

        program = (Elements.Program)parser.program().Accept(Rewriter);
        return true;
    }
}