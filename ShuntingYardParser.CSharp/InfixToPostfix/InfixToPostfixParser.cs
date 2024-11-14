using ShuntingYardParser.CSharp.Parser;
using ShuntingYardParser.CSharp.Lexer;

namespace ShuntingYardParser.CSharp.InfixToPostfix;

public class InfixToPostfixParser(ExpressionLexer lexer) : ShuntingYardParser<Token>(lexer)
{
    protected override void PushOperand(Token t) =>
        Operands.Push(t);

    protected override void ReduceExpression()
    {
        Token op = Operators.Pop();

        if (op.Type == TokenType.UnaryMinus)
        {
            Token operand = Operands.Pop();
            Operands.Push(new Token(TokenType.Literal, $"{operand.Lexeme} {op.Lexeme}"));
        }
        else
        {
            Token right = Operands.Pop();
            Token left = Operands.Pop();
            Operands.Push(new Token(TokenType.Literal, $"{left.Lexeme} {right.Lexeme} {op.Lexeme}"));
        }
    }
}