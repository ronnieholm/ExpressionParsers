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
        public Token Token { get; }
        public long Value { get; }
        public string String { get => $"{Token.Literal}"; }

        public IntegerLiteral(Token token, long value)
        {
            Token = token;
            Value = value;
        }
    }

    public class FloatLiteral : IExpression
    {
        public Token Token { get; }
        public double Value { get; }
        public string String { get => $"{Token.Literal}"; }

        public FloatLiteral(Token token, double value)
        {
            Token = token;
            Value = value;
        }
    }

    public class PrefixExpression : IExpression
    {
        public Token Token { get; }
        public TokenKind Operator { get; }
        public IExpression Right;
        public string String { get => $"({Token.Literal}{Right.String})"; }

        public PrefixExpression(Token token, TokenKind @operator, IExpression right)
        {
            Token = token;
            Operator = @operator;
            Right = right;
        }
    }

    public class InfixExpression : IExpression
    {
        public Token Token { get; }
        public IExpression Left;
        public TokenKind Operator { get; }
        public IExpression Right;
        public string String { get => $"({Left.String} {Token.Literal} {Right.String})"; }

        public InfixExpression(Token token, IExpression left, TokenKind @operator, IExpression right)
        {
            Token = token;
            Left = left;
            Operator = @operator;
            Right = right;
        }
    }

    public class PostfixExpression : IExpression
    {
        public Token Token { get; }
        public TokenKind Operator { get; }
        public IExpression Left;
        public string String { get => $"({Left.String}{Token.Literal})"; }

        public PostfixExpression(Token token, TokenKind @operator, IExpression left)
        {
            Token = token;
            Operator = @operator;
            Left = left;
        }
    }
}