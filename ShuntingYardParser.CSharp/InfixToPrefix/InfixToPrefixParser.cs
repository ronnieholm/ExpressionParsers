using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using ShuntingYardParser.CSharp.Lexer;
using ShuntingYardParser.CSharp.Parser;

namespace ShuntingYardParser.CSharp.InfixToPrefix {
    public class InfixToPrefixParser : ShuntingYardParser<Token> {
        public InfixToPrefixParser(ExpressionLexer lexer) : base(lexer) { }

        protected override void PushOperand(Token t) {
            Operands.Push(t);
        }

        protected override void ReduceExpression() {
            Token op = Operators.Pop();

            if (op.Type == TokenType.UnaryMinus) {
                Token operand = Operands.Pop();

                Operands.Push(new Token {
                    Type = TokenType.Literal,
                    Lexeme = string.Format(
                        "{0} {1}", op.Lexeme, operand.Lexeme)
                });
            }
            else {
                Token right = Operands.Pop();
                Token left = Operands.Pop();

                Operands.Push(new Token {
                    Type = TokenType.Literal,
                    Lexeme = string.Format(
                        "{0} {1} {2}", op.Lexeme, left.Lexeme, right.Lexeme)
                });
            }
        }
    }
}
