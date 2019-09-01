namespace PrattParser.Core
{
    // TODO: Create ctors for classes to make sure all properties are set (immutable)

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
        public string String { get => $"{Token.Literal}"; }
    }

    public class FloatLiteral : IExpression
    {
        public Token Token { get; set; }
        public double Value { get; set; }
        public string String { get => $"{Token.Literal}"; }
    }

    public class PrefixExpression : IExpression
    {
        public Token Token { get; set; }
        public TokenKind Operator { get; set; }
        public IExpression Right;
        public string String { get => $"({Token.Literal}{Right.String})"; }
    }

    public class InfixExpression : IExpression
    {
        public Token Token { get; set; }
        public IExpression Left;
        public TokenKind Operator { get; set; }
        public IExpression Right;
        public string String { get => $"({Left.String} {Token.Literal} {Right.String})"; }
    }

    public class PostfixExpression : IExpression
    {
        public Token Token { get; set; }
        public TokenKind Operator { get; set; }
        public IExpression Left;
        public string String { get => $"({Left.String}{Token.Literal})"; }        
    }
}