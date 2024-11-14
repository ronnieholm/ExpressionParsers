using ShuntingYardParser.CSharp.Parser;
using ShuntingYardParser.CSharp.Lexer;

namespace ShuntingYardParser.CSharp.InfixToAbstractSyntaxTree;

public class InfixToAbstractSyntaxTreeParser(ExpressionLexer lexer) : ShuntingYardParser<Expression>(lexer)
{
    protected override void PushOperand(Token t) =>
        Operands.Push(new Literal(int.Parse(t.Lexeme)));

    protected override void ReduceExpression()
    {
        Token op = Operators.Pop();
        Expression result;

        if (op.Type == TokenType.UnaryMinus)
        {
            Expression operand = Operands.Pop();
            result = new UnaryMinus(operand);
        }
        else
        {
            Expression right = Operands.Pop();
            Expression left = Operands.Pop();

            result = op.Type switch
            {
                TokenType.BinaryPlus => new BinaryPlus(left, right),
                TokenType.BinaryMinus => new BinaryMinus(left, right),
                TokenType.BinaryMul => new BinaryMul(left, right),
                TokenType.BinaryDiv => new BinaryDiv(left, right),
                TokenType.BinaryExp => new BinaryExp(left, right),
                _ => throw new ArgumentException($"Unsupported operator: {op.Lexeme}")
            };
        }

        Operands.Push(result);
    }
}