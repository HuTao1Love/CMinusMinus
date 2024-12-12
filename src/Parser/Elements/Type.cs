namespace Parser.Elements;


public abstract class Type : Element
{
    public enum ValueTypeType
    {
        Number,
        String,
        Bool,
    }

    public class ValueType : Type
    {
        public required ValueTypeType Type { get; set; }
    }
}