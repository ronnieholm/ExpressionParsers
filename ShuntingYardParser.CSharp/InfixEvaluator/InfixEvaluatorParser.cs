using ShuntingYardParser.CSharp.Lexer;
using ShuntingYardParser.CSharp.Parser;

namespace ShuntingYardParser.CSharp.InfixEvaluator;

public class InfixEvaluatorParser(ExpressionLexer lexer) : ShuntingYardParser<int>(lexer)
{
    protected override void PushOperand(Token t) =>
        Operands.Push(int.Parse(t.Lexeme));

    protected override void ReduceExpression()
    {
        Token op = Operators.Pop();

        if (op.Type == TokenType.UnaryMinus)
        {
            int operand = Operands.Pop();
            Operands.Push(-operand);
        }
        else
        {
            int right = Operands.Pop();
            int left = Operands.Pop();

            int result = op.Type switch
            {
                TokenType.BinaryPlus => left + right,
                TokenType.BinaryMinus => left - right,
                TokenType.BinaryMul => left * right,
                TokenType.BinaryDiv => left / right,
                TokenType.BinaryExp => (int)Math.Pow(left, right),
                _ => throw new ArgumentException($"Unsupported operator: {op.Lexeme}")
            };

            Operands.Push(result);
        }
    }
}