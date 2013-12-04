using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using ShuntingYardParser.Lexer;
using ShuntingYardParser.Parser;

namespace ShuntingYardParser.InfixEvaluator {
    public class InfixEvaluatorParser : ShuntingYardParser<int> {
        public InfixEvaluatorParser(ExpressionLexer lexer) : base(lexer) { }

        protected override void PushOperand(Token t) {
            Operands.Push(Int32.Parse(t.Lexeme));
        }

        protected override void ReduceExpression() {
            Token op = Operators.Pop();

            if (op.Type == TokenType.UnaryMinus) {
                int operand = Operands.Pop();
                Operands.Push(-operand);
            }
            else {
                int right = Operands.Pop();
                int left = Operands.Pop();

                int result = 0;
                switch (op.Type) {
                    case TokenType.BinaryPlus:
                        result = left + right;
                        break;
                    case TokenType.BinaryMinus:
                        result = left - right;
                        break;
                    case TokenType.BinaryMul:
                        result = left * right;
                        break;
                    case TokenType.BinaryDiv:
                        result = left / right;
                        break;
                    case TokenType.BinaryExp:
                        result = (int)Math.Pow((double)left, (double)right);
                        break;
                    default:
                        throw new ArgumentException(
                            string.Format("Unsupported operator: {0}", op.Lexeme));
                }

                Operands.Push(result);
            }
        }
    }
}
