namespace RecursiveDescentParser.Core;

// As with the Monkey language interpreter, we can traverse the AST without an
// explicit visitor interface. It then becomes each evaluator's responsibility
// to manually ensure that each type of AST node is evaluated. With the
// interface, the compiler will ensure that each method in the interface has an
// implementation and that each evaluator of the same AST has the same
// structure. 

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

// Only reason we keep pointers back to the original token is for location and
// lexeme information, if they're ever needed. For most parser, they can safely
// be omitted. 

public record IntegerLiteral(Token _, long Value) : IExpression
{
    public T Accept<T>(IExpressionVisitor<T> visitor) =>
        visitor.Visit(this);
}

public record FloatLiteral(Token _, double Value) : IExpression
{
    public T Accept<T>(IExpressionVisitor<T> visitor) =>
        visitor.Visit(this);
}

public record PrefixExpression(Token _, TokenKind Operator, IExpression Right) : IExpression
{
    public T Accept<T>(IExpressionVisitor<T> visitor) =>
        visitor.Visit(this);
}

public record InfixExpression(Token _, IExpression Left, TokenKind Operator, IExpression Right) : IExpression
{
    public T Accept<T>(IExpressionVisitor<T> visitor) =>
        visitor.Visit(this);
}