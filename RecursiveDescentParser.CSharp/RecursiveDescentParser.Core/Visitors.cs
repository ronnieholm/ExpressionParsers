using System.Globalization;

// TODO: graphviz dot file generator (graphviz.org)

namespace RecursiveDescentParser.Core;

public class InfixAstFlattener : IExpressionVisitor<string>
{
    public string Flatten(IExpression expr) => expr.Accept(this);
    public string Visit(IntegerLiteral literal) => literal.Value.ToString(CultureInfo.InvariantCulture);
    public string Visit(FloatLiteral literal) => literal.Value.ToString(CultureInfo.InvariantCulture);
    public string Visit(PrefixExpression expr) => $"({expr.Operator.ToFriendlyName()}{expr.Right.Accept(this)})";
    public string Visit(InfixExpression expr) => $"({expr.Left.Accept(this)} {expr.Operator.ToFriendlyName()} {expr.Right.Accept(this)})";
}

public class PrefixAstFlattener : IExpressionVisitor<string>
{
    public string Flatten(IExpression expr) => expr.Accept(this);

    public string Visit(IntegerLiteral literal) => literal.Value.ToString(CultureInfo.InvariantCulture);

    public string Visit(FloatLiteral literal) => literal.Value.ToString(CultureInfo.InvariantCulture);

    public string Visit(PrefixExpression expr) => $"{expr.Operator.ToFriendlyName()} {expr.Right.Accept(this)}";

    public string Visit(InfixExpression expr) => $"{expr.Operator.ToFriendlyName()} {expr.Left.Accept(this)} {expr.Right.Accept(this)}";
}

