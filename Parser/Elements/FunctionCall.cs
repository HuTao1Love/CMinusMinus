namespace Parser.Elements;

public class FunctionCall : Element
{
    public required string Function { get; set; }
    public required Expression[] Args { get; set; }
}
