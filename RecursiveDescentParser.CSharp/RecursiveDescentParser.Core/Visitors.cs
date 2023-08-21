using System;
using System.Diagnostics;
using System.Globalization;

// TODO: graphviz dot file generator (graphviz.org)

namespace RecursiveDescentParser.Core;

public class InfixAstFlattener : IExpressionVisitor<string>
{
    public string Evaluate(IExpression expr) => expr.Accept(this);
    public string Visit(IntegerLiteral literal) => literal.Value.ToString(CultureInfo.InvariantCulture);
    public string Visit(FloatLiteral literal) => literal.Value.ToString(CultureInfo.InvariantCulture);
    public string Visit(PrefixExpression expr) => $"({expr.Operator.ToFriendlyName()}{expr.Right.Accept(this)})";
    public string Visit(InfixExpression expr) => $"({expr.Left.Accept(this)} {expr.Operator.ToFriendlyName()} {expr.Right.Accept(this)})";
}

public class PrefixAstFlattener : IExpressionVisitor<string>
{
    public string Evaluate(IExpression expr) => expr.Accept(this);
    public string Visit(IntegerLiteral literal) => literal.Value.ToString(CultureInfo.InvariantCulture);
    public string Visit(FloatLiteral literal) => literal.Value.ToString(CultureInfo.InvariantCulture);
    public string Visit(PrefixExpression expr) => $"{expr.Operator.ToFriendlyName()} {expr.Right.Accept(this)}";
    public string Visit(InfixExpression expr) => $"{expr.Operator.ToFriendlyName()} {expr.Left.Accept(this)} {expr.Right.Accept(this)}";
}

public class PostfixAstFlattener : IExpressionVisitor<string>
{
    public string Evaluate(IExpression expr) => expr.Accept(this);
    public string Visit(IntegerLiteral literal) => literal.Value.ToString(CultureInfo.InvariantCulture);
    public string Visit(FloatLiteral literal) => literal.Value.ToString(CultureInfo.InvariantCulture);
    public string Visit(PrefixExpression expr) => $"{expr.Right.Accept(this)} {expr.Operator.ToFriendlyName()}";
    public string Visit(InfixExpression expr) => $"{expr.Left.Accept(this)} {expr.Right.Accept(this)} {expr.Operator.ToFriendlyName()}";
}

public class Interpreter : IExpressionVisitor<double>
{
    public double Evaluate(IExpression expr) => expr.Accept(this);
    public double Visit(IntegerLiteral literal) => literal.Value;
    public double Visit(FloatLiteral literal) => literal.Value;

    public double Visit(PrefixExpression expr) =>
        expr.Operator switch
        {
            TokenKind.Minus => -expr.Right.Accept(this),
            _ => throw new UnreachableException(expr.Operator.ToFriendlyName())
        };

    public double Visit(InfixExpression expr) =>
        expr.Operator switch
        {
            TokenKind.Plus => expr.Left.Accept(this) + expr.Right.Accept(this),
            TokenKind.Minus => expr.Left.Accept(this) - expr.Right.Accept(this),
            TokenKind.Multiplication => expr.Left.Accept(this) * expr.Right.Accept(this),
            TokenKind.Division => expr.Left.Accept(this) / expr.Right.Accept(this),
            TokenKind.Power => Math.Pow(expr.Left.Accept(this), expr.Right.Accept(this)),
            _ => throw new UnreachableException(expr.Operator.ToFriendlyName())
        };
}