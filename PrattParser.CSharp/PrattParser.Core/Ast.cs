// Extensions:
// - Add an Eval(Interpreter i) method to each AST node to make it more object
//   oriented.
// - Add a Dump method to each node as in
//   https://www.youtube.com/watch?v=byNwCHc_IIM, around 1h19m

namespace PrattParser.Core;

public interface IExpression
{
    // We don't override Object.ToString() because we want to keep the
    // String call explicit.
    string String { get; }
}

public record IntegerLiteral(Token Token, long Value) : IExpression
{
    public string String { get => $"{Token.Literal}"; }
}

public record FloatLiteral(Token Token, double Value) : IExpression
{
    public string String { get => $"{Token.Literal}"; }
}

public record PrefixExpression(Token Token, TokenKind Operator, IExpression Right) : IExpression
{
    public string String { get => $"({Token.Literal}{Right.String})"; }
}

public record InfixExpression(Token Token, IExpression Left, TokenKind Operator, IExpression Right) : IExpression
{
    public string String { get => $"({Left.String} {Token.Literal} {Right.String})"; }
}

public record PostfixExpression(Token Token, TokenKind Operator, IExpression Left) : IExpression
{
    public string String { get => $"({Left.String}{Token.Literal})"; }
}