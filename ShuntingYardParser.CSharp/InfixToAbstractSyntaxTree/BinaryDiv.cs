namespace ShuntingYardParser.CSharp.InfixToAbstractSyntaxTree;

// Considerations:
//   - The return type of Evaluate() ideally shouldn't be fixed, but generics
//     doesn't allow us to constrain the type parameter to only those types
//     that have arithmetic operators defined.

// Improvements:
//   - Change return type of Evaluate() to floating point to support more
//     accurate computations, e.g., 2^-1 yields 0 with integer math.

public abstract record Expression
{
    public abstract int Evaluate();
}

public record BinaryDiv(Expression LeftOperand, Expression RightOperand) : Expression
{
    public override int Evaluate() => LeftOperand.Evaluate() / RightOperand.Evaluate();
}

public record BinaryExp(Expression LeftOperand, Expression RightOperand) : Expression
{
    public override int Evaluate() => (int)Math.Pow(LeftOperand.Evaluate(), RightOperand.Evaluate());
}

public record BinaryMinus(Expression LeftOperand, Expression RightOperand) : Expression
{
    public override int Evaluate() => LeftOperand.Evaluate() - RightOperand.Evaluate();
}

public record BinaryMul(Expression LeftOperand, Expression RightOperand) : Expression
{
    public override int Evaluate() => LeftOperand.Evaluate() * RightOperand.Evaluate();
}

public record BinaryPlus(Expression LeftOperand, Expression RightOperand) : Expression
{
    public override int Evaluate() => LeftOperand.Evaluate() + RightOperand.Evaluate();
}