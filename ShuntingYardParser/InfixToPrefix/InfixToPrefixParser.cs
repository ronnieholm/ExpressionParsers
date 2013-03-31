using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using ShuntingYardParser.Lexer;
using ShuntingYardParser.Parser;

namespace ShuntingYardParser.InfixToPrefix {
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
                    Value = string.Format(
                        "{0} {1}", op.Value, operand.Value)
                });
            }
            else {
                Token right = Operands.Pop();
                Token left = Operands.Pop();

                Operands.Push(new Token {
                    Type = TokenType.Literal,
                    Value = string.Format(
                        "{0} {1} {2}", op.Value, left.Value, right.Value)
                });
            }
        }
    }
}
