// Considerations:
//   - Returning tokens using IEnumerable and yield instead of List will not work
//     as parsing unary minus requires looking at the previously matched token
//     and IEnumerable doesn't support this. We could store the previously matched
//     tokens in a field, but it's hardly worth the added complexity.

// Improvements:
//   - Support floating point numbers and function names and arguments
//   - Use regular expressions instead of implementing our own state machine

namespace ShuntingYardParser.CSharp.Lexer;

public class ExpressionLexer
{
    public static List<Token> Tokenize(string expression)
    {
        List<Token> tokens = new List<Token>();
        var lexeme = "";

        foreach (char c in expression)
        {
            if (char.IsDigit(c))
            {
                lexeme += c;
                continue;
            }

            if (c is '+' or '-' or '*' or '/' or '^' or '(' or ')')
            {
                if (lexeme.Length > 0)
                {
                    tokens.Add(new Token(TokenType.Literal, lexeme));
                    lexeme = "";
                }
                if (c == '+')
                    tokens.Add(new Token(TokenType.BinaryPlus, "+"));
                if (c == '-')
                {
                    if (tokens.Count == 0 ||
                        tokens.Last().Type == TokenType.BinaryDiv ||
                        tokens.Last().Type == TokenType.BinaryExp ||
                        tokens.Last().Type == TokenType.BinaryMinus ||
                        tokens.Last().Type == TokenType.BinaryMul ||
                        tokens.Last().Type == TokenType.BinaryPlus ||
                        tokens.Last().Type == TokenType.LeftParen)
                    {
                        tokens.Add(new Token(TokenType.UnaryMinus, "-"));
                    }
                    else if (tokens.Last().Type == TokenType.Literal ||
                             tokens.Last().Type == TokenType.RightParen)
                    {
                        tokens.Add(new Token(TokenType.BinaryMinus, "-"));
                    }
                }
                if (c == '*')
                    tokens.Add(new Token(TokenType.BinaryMul, "*"));
                if (c == '/')
                    tokens.Add(new Token(TokenType.BinaryDiv, "/"));
                if (c == '(')
                    tokens.Add(new Token(TokenType.LeftParen, "("));
                if (c == ')')
                    tokens.Add(new Token(TokenType.RightParen, ")"));
                if (c == '^')
                    tokens.Add(new Token(TokenType.BinaryExp, "^"));
            }
        }

        if (lexeme.Length > 0)
            tokens.Add(new Token(TokenType.Literal, lexeme));

        tokens.Add(new Token(TokenType.End, ""));
        return tokens;
    }
}