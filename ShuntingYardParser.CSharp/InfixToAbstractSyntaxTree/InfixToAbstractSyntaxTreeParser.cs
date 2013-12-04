using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShuntingYardParser.CSharp.Parser;
using ShuntingYardParser.CSharp.Lexer;

namespace ShuntingYardParser.CSharp.InfixToAbstractSyntaxTree {
    public class InfixToAbstractSyntaxTreeParser : ShuntingYardParser<Expression> {
        public InfixToAbstractSyntaxTreeParser(ExpressionLexer lexer) : base(lexer) { }

        protected override void PushOperand(Token t) {
            Operands.Push(new Literal { Value = Int32.Parse(t.Lexeme) });
        }

        protected override void ReduceExpression() {
            Token op = Operators.Pop();
            Expression result = null;

            if (op.Type == TokenType.UnaryMinus) {
                Expression operand = Operands.Pop();
                result = new UnaryMinus { Operand = operand };
                Operands.Push(result);
            }
            else {
                Expression right = Operands.Pop();
                Expression left = Operands.Pop();

                switch (op.Type) {
                    case TokenType.BinaryPlus:
                        result = new BinaryPlus { LeftOperand = left, RightOperand = right };
                        break;
                    case TokenType.BinaryMinus:
                        result = new BinaryMinus { LeftOperand = left, RightOperand = right };
                        break;
                    case TokenType.BinaryMul:
                        result = new BinaryMul { LeftOperand = left, RightOperand = right };
                        break;
                    case TokenType.BinaryDiv:
                        result = new BinaryDiv { LeftOperand = left, RightOperand = right };
                        break;
                    case TokenType.BinaryExp:
                        result = new BinaryExp { LeftOperand = left, RightOperand = right };
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
