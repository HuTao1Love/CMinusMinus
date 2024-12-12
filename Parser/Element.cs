namespace Parser;

public interface IElement
{
}

public abstract class Element
{
}

public interface IValue : IElement
{
    public object Value { get; set; }
}

public interface IValue<T> : IValue
{
#pragma warning disable CS0108, CS0114
    public T Value { get; set; }
#pragma warning restore CS0108, CS0114

    object IValue.Value
    {
        get => Value!;
        set => Value = (T)value;
    }
}

public class Bool : Element, IValue<bool>
{
    public bool Value { get; set; }
}

public class String : Element, IValue<string>
{
    public string Value { get; set; }
}

public class Number : Element, IValue<long>
{
    public long Value { get; set; }
}

public class Variable : Element
{
    public string Name { get; set; }
    public IValue Value { get; set; }
}

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
        public ValueTypeType Type { get; set; }
    }
}

public abstract class ReturnType : Element
{
    public class ReturnVoidType : ReturnType
    {
    }

    public class ReturnSomeType : ReturnType
    {
        public Type Type { get; set; }
    }
}

public abstract class Statement : Element
{
    public class IfStatement : Statement
    {
        public Expression Expression { get; set; }
        public Block If { get; set; }
        public Block? Else { get; set; }
    }

    public class InitializationStatement : Statement
    {
        public Type Type { get; set; }
        public string Variable { get; set; }
        public Expression? Value { get; set; }
    }

    public class AssignmentStatement : Statement
    {
        public string Variable { get; set; }
        public Expression Value { get; set; }
    }

    public class ReturnStatement : Statement
    {
        public Expression Value { get; set; }
    }

    public class FunctionCallStatement : Statement
    {
        public FunctionCall FunctionCall { get; set; }
    }
}

public abstract class Expression : Element
{
    public class BinaryExpression : Expression
    {
        public Expression Left { get; set; }
        public Expression Right { get; set; }

        public class LogicalExpression : BinaryExpression
        {
            public string Operator { get; set; }
        }

        public class CalcExpression : BinaryExpression
        {
            public string Operator { get; set; }
        }
    }

    public class ValueExpression : Expression
    {
        public IValue Value { get; set; }
    }

    public class FunctionCallExpression : Expression
    {
        public FunctionCall Value { get; set; }
    }

    public class VariableExpression : Expression
    {
        public string Variable { get; set; }
    }

    public class NegationExpression : Expression
    {
        public Expression Expression { get; set; }
    }
}

public class Block : Element
{
    public Statement[] Statements { get; set; }
}

public class Function : Element
{
    public ReturnType Type { get; set; }
    public string Name { get; set; }
    public string[] Args { get; set; }
    public Block Block { get; set; }
}

public class FunctionCall : Element
{
    public string Function { get; set; }
    public Expression[] Args { get; set; }
}

public class Program : Element
{
    public Function[] Functions { get; set; }
}