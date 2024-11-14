using ShuntingYardParser.CSharp.Lexer;
using ShuntingYardParser.CSharp.Parser;

namespace ShuntingYardParser.CSharp.InfixToPrefix;

public class InfixToPrefixParser(ExpressionLexer lexer) : ShuntingYardParser<Token>(lexer)
{
    protected override void PushOperand(Token t) =>
        Operands.Push(t);

    protected override void ReduceExpression()
    {
        Token op = Operators.Pop();

        if (op.Type == TokenType.UnaryMinus)
        {
            Token operand = Operands.Pop();
            Operands.Push(new Token(TokenType.Literal, $"{op.Lexeme} {operand.Lexeme}"));
        }
        else
        {
            Token right = Operands.Pop();
            Token left = Operands.Pop();
            Operands.Push(new Token(TokenType.Literal, $"{op.Lexeme} {left.Lexeme} {right.Lexeme}"));
        }
    }
}