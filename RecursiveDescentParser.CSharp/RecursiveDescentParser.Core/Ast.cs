namespace RecursiveDescentParser.Core;

public interface IExpression
{
}

public record IntegerLiteral(Token Token, long Value) : IExpression
{
}

public record FloatLiteral(Token Token, double Value) : IExpression
{
}

public record PrefixExpression(Token Token, TokenKind Operator, IExpression Right) : IExpression
{
}

public record InfixExpression(Token Token, IExpression Left, TokenKind Operator, IExpression Right) : IExpression
{
}

public record PostfixExpression(Token Token, TokenKind Operator, IExpression Left) : IExpression
{
}