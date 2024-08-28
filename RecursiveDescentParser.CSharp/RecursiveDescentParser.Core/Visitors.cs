using System;
using System.Diagnostics;
using System.Globalization;

namespace RecursiveDescentParser.Core;

// Strictly speaking, we don't require the Evaluate method on each IExpressionVisitor
// implementation, and it isn't part of the interface. Instead we could call the Visit
// method and correct overload is determined by the compiler. The Evaluate method is
// useful, though, in contexts which require initial setup.

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

    public double Visit(InfixExpression expr)
    {
        var left = expr.Left.Accept(this);
        var right = expr.Right.Accept(this);
        return expr.Operator switch
        {
            TokenKind.Plus => left + right,
            TokenKind.Minus => left - right,
            TokenKind.Multiplication => left * right,
            TokenKind.Division => left / right,
            TokenKind.Power => Math.Pow(left, right),
            _ => throw new UnreachableException(expr.Operator.ToFriendlyName())
        };
    }
}

// TODO: graphviz dot file generator (graphviz.org)

public class GraphvizVisualizer : IExpressionVisitor<string>
{
    public string Evaluate(IExpression expr) => expr.Accept(this);

    public string Visit(IntegerLiteral literal)
    {
        throw new NotImplementedException();
    }

    public string Visit(FloatLiteral literal)
    {
        throw new NotImplementedException();
    }

    public string Visit(PrefixExpression expr)
    {
        throw new NotImplementedException();
    }

    public string Visit(InfixExpression expr)
    {
        throw new NotImplementedException();
    }
}