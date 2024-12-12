namespace Parser.Elements;

public interface IValue : IElement
{
    public object Value { get; set; }
}

public interface IValue<T> : IValue
{
    // keyword new is not required there
#pragma warning disable CS0108, CS0114
    public T Value { get; set; }
#pragma warning restore CS0108, CS0114

    object IValue.Value { get => Value!; set => Value = (T)value; }
}

public class Bool : Element, IValue<bool>
{
    public required bool Value { get; set; }
}

public class String : Element, IValue<string>
{
    public required string Value { get; set; }
}

public class Number : Element, IValue<long>
{
    public required long Value { get; set; }
}