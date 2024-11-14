using ShuntingYardParser.CSharp.Lexer;

// Improvements:
//   - Support floating point numbers and function names and arguments

namespace ShuntingYardParser.CSharp.Parser;

public abstract class ShuntingYardParser<TOperand>(ExpressionLexer lexer)
{
    private readonly Dictionary<TokenType, int> _precedence = new()
    {
        { TokenType.BinaryExp, 4 },
        { TokenType.UnaryMinus, 3 },
        { TokenType.BinaryMul, 2 },
        { TokenType.BinaryDiv, 2 },
        { TokenType.BinaryPlus, 1 },
        { TokenType.BinaryMinus, 1 },
    };

    private enum Associativity {
        Left,
        Right
    }

    private readonly Dictionary<TokenType, Associativity> _associativity = new()
    {
        { TokenType.BinaryExp, Associativity.Right },
        { TokenType.UnaryMinus, Associativity.Left },
        { TokenType.BinaryMul, Associativity.Left },
        { TokenType.BinaryDiv, Associativity.Left },
        { TokenType.BinaryPlus, Associativity.Left },
        { TokenType.BinaryMinus, Associativity.Left }
    };

    protected Stack<Token> Operators { get; } = new();
    protected Stack<TOperand> Operands { get; } = new();

    private static bool IsBinaryOperator(Token t) {
        return t.Type is TokenType.BinaryExp
            or TokenType.UnaryMinus
            or TokenType.BinaryMul
            or TokenType.BinaryDiv
            or TokenType.BinaryPlus
            or TokenType.BinaryMinus;
    }

    public TOperand Parse(string expression) {
        List<Token> tokens = ExpressionLexer.Tokenize(expression);
        var tokenNumber = 0;

        while (tokens[tokenNumber].Type != TokenType.End) {
            // read a token
            var token = tokens[tokenNumber];

            // if the token is a operand, then push it to the operand stack
            if (token.Type == TokenType.Literal) {
                PushOperand(token);
            }
            // if the token is a unary prefix operator, push it on to the operator stack
            if (token.Type == TokenType.UnaryMinus) {
                PushOperator(token);
            }

            // if the token is a binary operator, o1, then
            else if (IsBinaryOperator(token)) {
                // while there is an operator token, o2, at the top of the operator stack, and
                //     either o1 is left-associative and its precedence is less than or equal to that of o2
                //         or o1 is right-associative and its precedence less than that of o2
                //   call reduce expression
                // push o1 onto the operator stack
                while (Operators.Count > 0 && IsBinaryOperator(Operators.Peek()) &&
                       (_associativity[token.Type] == Associativity.Left &&
                        _precedence[token.Type] <= _precedence[Operators.Peek().Type] ||
                        _associativity[token.Type] == Associativity.Right &&
                        _precedence[token.Type] < _precedence[Operators.Peek().Type])) {
                    ReduceExpression();
                }
                PushOperator(token);
            }
            // if the token is a left parenthesis, then push it onto the operator stack
            else if (token.Type == TokenType.LeftParen) {
                Operators.Push(token);
            }
            // if the token is a right parenthesis
            else if (token.Type == TokenType.RightParen) {
                // until the token at the top of the operator stack is a left parenthesis
                while (Operators.Count != 0 && Operators.Peek().Type != TokenType.LeftParen) {
                    // call reduce expression
                    ReduceExpression();
                }

                // if the operator stack runs out without finding a left parenthesis, then there are mismatched parentheses
                if (Operators.Count == 0) {
                    throw new ArgumentException("Unmatched parenthesis");
                }

                // pop the left parenthesis from the stack
                if (Operators.Peek().Type == TokenType.LeftParen) {
                    Operators.Pop();
                }
            }

            tokenNumber++;
        }

        // when there are no more tokens to read
        //     while there are still operator tokens in the operator stack
        while (Operators.Count > 0) {
            // if the operator token on the top of the stack is a parenthesis, then there are mismatched parentheses
            if (Operators.Peek().Type == TokenType.LeftParen ||
                Operators.Peek().Type == TokenType.RightParen) {
                throw new ArgumentException("Unmatched parenthesis");
            }

            // call reduce expression
            ReduceExpression();
        }

        return Operands.Pop();
    }

    private void PushOperator(Token t) {
        Operators.Push(t);
    }

    protected abstract void PushOperand(Token t);
    protected abstract void ReduceExpression();
}