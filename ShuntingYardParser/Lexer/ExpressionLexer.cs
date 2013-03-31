using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShuntingYardParser.Lexer {
    public class ExpressionLexer {
        public List<Token> Tokenize(string expression) {
            List<Token> tokens = new List<Token>();
            bool inToken = false;
            string currentToken = "";

            foreach (char c in expression) {
                if (char.IsDigit(c)) {
                    inToken = true;
                    currentToken += c;
                    continue;
                }
                else if (c == '+' || c == '-' || c == '*' || c == '/' || c == '^' || c == '(' || c == ')') {
                    if (inToken) {
                        tokens.Add(new Token { Type = TokenType.Literal, Value = currentToken });
                        currentToken = "";
                        inToken = false;
                    }
                    if (c == '+') {
                        tokens.Add(new Token { Type = TokenType.BinaryPlus, Value = "+" });
                    }
                    if (c == '-') {
                        if (tokens.Count() == 0 ||
                            tokens.Last().Type == TokenType.BinaryDiv ||
                            tokens.Last().Type == TokenType.BinaryExp ||
                            tokens.Last().Type == TokenType.BinaryMinus ||
                            tokens.Last().Type == TokenType.BinaryMul ||
                            tokens.Last().Type == TokenType.BinaryPlus ||
                            tokens.Last().Type == TokenType.LeftParen) {
                            tokens.Add(new Token { Type = TokenType.UnaryMinus, Value = "-" });
                        }
                        else if (tokens.Last().Type == TokenType.Literal ||
                            tokens.Last().Type == TokenType.RightParen) {
                            tokens.Add(new Token { Type = TokenType.BinaryMinus, Value = "-" });
                        }
                    }
                    if (c == '*') {
                        tokens.Add(new Token { Type = TokenType.BinaryMul, Value = "*" });
                    }
                    if (c == '/') {
                        tokens.Add(new Token { Type = TokenType.BinaryDiv, Value = "/" });
                    }
                    if (c == '(') {
                        tokens.Add(new Token { Type = TokenType.LeftParen, Value = "(" });
                    }
                    if (c == ')') {
                        tokens.Add(new Token { Type = TokenType.RightParen, Value = ")" });
                    }
                    if (c == '^') {
                        tokens.Add(new Token { Type = TokenType.BinaryExp, Value = "^" });
                    }
                }
            }

            if (inToken) {
                tokens.Add(new Token { Type = TokenType.Literal, Value = currentToken });
            }

            tokens.Add(new Token { Type = TokenType.End });
            return tokens;
        }
    }
}
