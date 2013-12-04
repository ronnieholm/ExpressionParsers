using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShuntingYardParser.CSharp.InfixToAbstractSyntaxTree {
    public class FlatExpressionPrinter {
        public string Print(Expression e) {
            if (e is UnaryMinus) {
                UnaryMinus x = e as UnaryMinus;
                return string.Format("-({0})", Print(x.Operand));
            }
            else if (e is BinaryPlus) {
                BinaryPlus x = e as BinaryPlus;
                return string.Format("+({0}, {1})", Print(x.LeftOperand), Print(x.RightOperand));
            }
            else if (e is BinaryMinus) {
                BinaryMinus x = e as BinaryMinus;
                return string.Format("-({0}, {1})", Print(x.LeftOperand), Print(x.RightOperand));
            }
            else if (e is BinaryMul) {
                BinaryMul x = e as BinaryMul;
                return string.Format("*({0}, {1})", Print(x.LeftOperand), Print(x.RightOperand));
            }
            else if (e is BinaryDiv) {
                BinaryDiv x = e as BinaryDiv;
                return string.Format("/({0}, {1})", Print(x.LeftOperand), Print(x.RightOperand));

            }
            else if (e is BinaryExp) {
                BinaryExp x = e as BinaryExp;
                return string.Format("^({0}, {1})", Print(x.LeftOperand), Print(x.RightOperand));

            }
            else if (e is Literal) {
                Literal x = e as Literal;
                return x.Value.ToString();
            }
            else {
                throw new ArgumentException("Unsupported type: " + e);
            }
        }
    }
}
