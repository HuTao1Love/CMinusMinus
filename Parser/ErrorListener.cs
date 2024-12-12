using Antlr4.Runtime;

namespace Parser;

public record Error(int Line, int Position, string Message);

public sealed class ErrorListener : IAntlrErrorListener<IToken>, IAntlrErrorListener<int>
{
    private readonly List<Error> _errors = [];

    public IReadOnlyCollection<Error> Errors => _errors;

    public void SyntaxError(
        TextWriter output, 
        IRecognizer recognizer, 
        IToken offendingSymbol, 
        int line, 
        int charPositionInLine, 
        string msg, 
        RecognitionException e)
    {
        _errors.Add(new Error(line, charPositionInLine, msg));
    }

    public void SyntaxError(
        TextWriter output, 
        IRecognizer recognizer, 
        int offendingSymbol, 
        int line, 
        int charPositionInLine, 
        string msg, 
        RecognitionException e)
    {
        _errors.Add(new Error(line, charPositionInLine, msg));
    }
}