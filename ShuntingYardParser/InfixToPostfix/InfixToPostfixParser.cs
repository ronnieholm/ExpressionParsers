using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using ShuntingYardParser.Parser;
using ShuntingYardParser.Lexer;

namespace ShuntingYardParser.InfixToPostfix {
    public class InfixToPostfixParser : ShuntingYardParser<Token> {
        public InfixToPostfixParser(ExpressionLexer lexer) : base(lexer) { }

        protected override void PushOperand(Token t) {
            Operands.Push(t);
        }

        protected override void ReduceExpression() {
            Token op = Operators.Pop();

            if (op.Type == TokenType.UnaryMinus) {
                Token operand = Operands.Pop();

                Operands.Push(new Token {
                    Type = TokenType.Literal,
                    Value = string.Format(
                        "{0} {1}", operand.Value, op.Value )
                });
            }
            else {
                Token right = Operands.Pop();
                Token left = Operands.Pop();

                Operands.Push(new Token {
                    Type = TokenType.Literal,
                    Value = string.Format(
                        "{0} {1} {2}", left.Value, right.Value, op.Value)
                });
            }
        }
    }
}
