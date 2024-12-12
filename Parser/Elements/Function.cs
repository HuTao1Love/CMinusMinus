namespace Parser.Elements;

public abstract class ReturnType : Element
{
    public class ReturnVoidType : ReturnType
    {
    }

    public class ReturnSomeType : ReturnType
    {
        public required Type Type { get; set; }
    }
}

public class Function : Element
{
    public required ReturnType Type { get; set; }
    public required string Name { get; set; }
    public required string[] Args { get; set; }
    public required Block Block { get; set; }
}
