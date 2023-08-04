namespace ShuntingYardParser.CSharp.InfixToAbstractSyntaxTree;

public record Literal(int Value) : Expression
{
    public override int Evaluate() => Value;
}