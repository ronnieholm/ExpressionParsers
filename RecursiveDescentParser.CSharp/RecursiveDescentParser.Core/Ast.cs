namespace RecursiveDescentParser.Core;

public interface IExpressionVisitor<out T>
{
    T Visit(IntegerLiteral literal);
    T Visit(FloatLiteral literal);
    T Visit(PrefixExpression expr);
    T Visit(InfixExpression expr);
}

public interface IExpression
{
    T Accept<T>(IExpressionVisitor<T> visitor);
}

public record IntegerLiteral(Token Token, long Value) : IExpression
{
    public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
}

public record FloatLiteral(Token Token, double Value) : IExpression
{
    public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
}

public record PrefixExpression(Token Token, TokenKind Operator, IExpression Right) : IExpression
{
    public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
}

public record InfixExpression(Token Token, IExpression Left, TokenKind Operator, IExpression Right) : IExpression
{
    public T Accept<T>(IExpressionVisitor<T> visitor) => visitor.Visit(this);
}