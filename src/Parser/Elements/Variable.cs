namespace Parser.Elements;

public class Variable : Element
{
    public required string Name { get; set; }
    public required IValue Value { get; set; }
}
