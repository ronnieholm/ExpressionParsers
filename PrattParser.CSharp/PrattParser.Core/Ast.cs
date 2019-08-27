namespace PrattParser.Core
{
    public interface IExpression
    {
        // We don't override Object.ToString() because we want to make the
        // String call explicit.
        string String { get; }
    }

    public class IntegerLiteral : IExpression
    {
        public Token Token { get; set; }
        public long Value { get; set; }
        public string TokenLiteral { get => Token.Literal; }
        public string String { get => $"{Value.ToString()}i"; }
    }

    public class FloatLiteral : IExpression
    {
        public Token Token { get; set; }
        public double Value { get; set; }
        public string TokenLiteral { get => Token.Literal; }
        public string String { get => $"{Value.ToString()}f"; }
    }

    public class PrefixExpression : IExpression
    {
        public Token Token { get; set; }
        public string Operator { get; set; }
        public IExpression Right;
        public string TokenLiteral { get => Token.Literal; }
        public string String { get => $"({Operator}{Right.String})"; }
    }

    public class InfixExpression : IExpression
    {
        public Token Token { get; set; }
        public IExpression Left;
        public string Operator { get; set; }
        public IExpression Right;
        public string TokenLiteral { get => Token.Literal; }
        public string String { get => $"({Left.String} {Operator} {Right.String})"; }
    }
}