namespace Parser.Elements;

public abstract class Expression : Element
{
    public class BinaryExpression : Expression
    {
        public required Expression Left { get; set; }
        public required Expression Right { get; set; }

        public class LogicalExpression : BinaryExpression
        {
            public required string Operator { get; set; }
        }

        public class CalcExpression : BinaryExpression
        {
            public required string Operator { get; set; }
        }
    }

    public class ValueExpression : Expression
    {
        public required IValue Value { get; set; }
    }

    public class FunctionCallExpression : Expression
    {
        public required FunctionCall Value { get; set; }
    }

    public class VariableExpression : Expression
    {
        public required string Variable { get; set; }
    }

    public class NegationExpression : Expression
    {
        public required Expression Expression { get; set; }
    }
}
