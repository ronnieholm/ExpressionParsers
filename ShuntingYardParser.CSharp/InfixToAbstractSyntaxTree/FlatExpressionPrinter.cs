namespace ShuntingYardParser.CSharp.InfixToAbstractSyntaxTree;

public static class FlatExpressionPrinter
{
    public static string Print(Expression e)
    {
        return e switch
        {
            UnaryMinus minus => $"-({Print(minus.Operand)})",
            BinaryPlus plus => $"+({Print(plus.LeftOperand)}, {Print(plus.RightOperand)})",
            BinaryMinus binaryMinus => $"-({Print(binaryMinus.LeftOperand)}, {Print(binaryMinus.RightOperand)})",
            BinaryMul mul => $"*({Print(mul.LeftOperand)}, {Print(mul.RightOperand)})",
            BinaryDiv div => $"/({Print(div.LeftOperand)}, {Print(div.RightOperand)})",
            BinaryExp exp => $"^({Print(exp.LeftOperand)}, {Print(exp.RightOperand)})",
            Literal literal => literal.Value.ToString(),
            _ => throw new ArgumentException("Unsupported type: " + e)
        };
    }
}