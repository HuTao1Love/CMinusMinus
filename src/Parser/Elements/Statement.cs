namespace Parser.Elements;

public abstract class Statement : Element
{
    public class IfStatement : Statement
    {
        public required Expression Expression { get; set; }
        public required Block If { get; set; }
        public required Block? Else { get; set; }
    }

    public class InitializationStatement : Statement
    {
        public required Type Type { get; set; }
        public required string Variable { get; set; }
        public required Expression? Value { get; set; }
    }

    public class AssignmentStatement : Statement
    {
        public required string Variable { get; set; }
        public required Expression Value { get; set; }
    }

    public class ReturnStatement : Statement
    {
        public required Expression Value { get; set; }
    }

    public class FunctionCallStatement : Statement
    {
        public required FunctionCall FunctionCall { get; set; }
    }
}
