using System.Diagnostics.CodeAnalysis;
using Antlr4.Runtime;
using Parser.Grammar;

namespace Parser;

public static class Parser
{
    private static readonly ElementBottomUpRewriter Rewriter = new();

    public static Program Parse(string text)
    {
        if (TryParse(text, out var program, out var errors))
        {
            return program;
        }

        throw new ApplicationException($"[Compilation Failed]\n" +
                                       $"Errors:\n" + 
                                       string.Join("\n", errors.Select(e => $"[{e.Line}:{e.Position}] {e.Message}")));
    }

    public static bool TryParse(
        string text, [NotNullWhen(true)] out Program? program, [NotNullWhen(false)] out IEnumerable<Error>? errors)
    {
        program = null;
        errors = null;

        var errorListener = new ErrorListener();

        var inputStream = new AntlrInputStream(text);
        var lexer = new CmmLexer(inputStream);
        lexer.AddErrorListener(errorListener);
        var cts = new CommonTokenStream(lexer);
        var parser = new CmmParser(cts);
        parser.AddErrorListener(errorListener);

        if (errorListener.Errors.Any())
        {
            errors = errorListener.Errors;
            return false;
        }

        program = (Program)parser.program().Accept(Rewriter);

        return true;
    }
}