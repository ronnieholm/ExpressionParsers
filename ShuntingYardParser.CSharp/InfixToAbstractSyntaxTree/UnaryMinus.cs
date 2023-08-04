namespace ShuntingYardParser.CSharp.InfixToAbstractSyntaxTree;

public record UnaryMinus(Expression Operand) : Expression
{
    public override int Evaluate() => -Operand.Evaluate();
}