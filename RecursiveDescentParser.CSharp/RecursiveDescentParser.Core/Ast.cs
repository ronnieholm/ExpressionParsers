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

// Only reason we keep pointers back to the original token is for
// location and lexeme information, if they're ever needed. For
// most parser, they can safely be omitted. 

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