using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Considerations:
//   - Returning tokens using IEnumerable and yield instead of List will not work
//     as parsing unary minus requires looking at the previously matched token
//     and IEnumerable doesn't support this. We could store the previously matched
//     tokens in a field, but it's hardly worth the added complexity.

// Improvements
//   - Support floating point numbers and function names and arguments
//   - Use regular expressions instead of implementing our own state machine

namespace ShuntingYardParser.Lexer {
    public class ExpressionLexer {
        public List<Token> Tokenize(string expression) {
            List<Token> _tokens = new List<Token>();
            string _lexeme = "";

            foreach (char c in expression) {
                if (char.IsDigit(c)) {
                    _lexeme += c;
                    continue;
                }
                else if (c == '+' || c == '-' || c == '*' || c == '/' || c == '^' || c == '(' || c == ')') {
                    if (_lexeme.Length > 0) {
                        _tokens.Add(new Token { Type = TokenType.Literal, Lexeme = _lexeme });
                        _lexeme = "";
                    }
                    if (c == '+') {
                        _tokens.Add(new Token { Type = TokenType.BinaryPlus, Lexeme = "+" });
                    }
                    if (c == '-') {
                        if (_tokens.Count() == 0 ||
                            _tokens.Last().Type == TokenType.BinaryDiv ||
                            _tokens.Last().Type == TokenType.BinaryExp ||
                            _tokens.Last().Type == TokenType.BinaryMinus ||
                            _tokens.Last().Type == TokenType.BinaryMul ||
                            _tokens.Last().Type == TokenType.BinaryPlus ||
                            _tokens.Last().Type == TokenType.LeftParen) {
                            _tokens.Add(new Token { Type = TokenType.UnaryMinus, Lexeme = "-" });
                        }
                        else if (_tokens.Last().Type == TokenType.Literal ||
                            _tokens.Last().Type == TokenType.RightParen) {
                            _tokens.Add(new Token { Type = TokenType.BinaryMinus, Lexeme = "-" });
                        }
                    }
                    if (c == '*') {
                        _tokens.Add(new Token { Type = TokenType.BinaryMul, Lexeme = "*" });
                    }
                    if (c == '/') {
                        _tokens.Add(new Token { Type = TokenType.BinaryDiv, Lexeme = "/" });
                    }
                    if (c == '(') {
                        _tokens.Add(new Token { Type = TokenType.LeftParen, Lexeme = "(" });
                    }
                    if (c == ')') {
                        _tokens.Add(new Token { Type = TokenType.RightParen, Lexeme = ")" });
                    }
                    if (c == '^') {
                        _tokens.Add(new Token { Type = TokenType.BinaryExp, Lexeme = "^" });
                    }
                }
            }

            if (_lexeme.Length > 0) {
                _tokens.Add(new Token { Type = TokenType.Literal, Lexeme = _lexeme });
            }

            _tokens.Add(new Token { Type = TokenType.End });
            return _tokens;
        }
    }
}
