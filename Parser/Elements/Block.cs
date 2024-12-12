namespace Parser.Elements;

public class Block : Element
{
    public required Statement[] Statements { get; set; }
}