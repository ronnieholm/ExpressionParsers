using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShuntingYardParser.InfixToAbstractSyntaxTree {
    public class HierarchicalExpressionPrinter {
        const int DefaultIndentation = 4;

        string _result;
        
        public string Print(Expression e) {
            Print(e, 0);
            return _result;
        }

        private void AddLine(string s) {
            _result += s + System.Environment.NewLine;
        }

        private void Print(Expression e, int indentation) {
            if (e is UnaryMinus) {
                UnaryMinus x = e as UnaryMinus;
                AddLine(new String(' ', indentation) + x.GetType().Name);
                Print(x.Operand, indentation + DefaultIndentation);
            }
            else if (e is BinaryPlus) {
                BinaryPlus x = e as BinaryPlus;
                AddLine(new String(' ', indentation) + x.GetType().Name);
                Print(x.LeftOperand, indentation + DefaultIndentation);
                Print(x.RightOperand, indentation + DefaultIndentation);
            }
            else if (e is BinaryMinus) {
                BinaryMinus x = e as BinaryMinus;
                AddLine(new String(' ', indentation) + x.GetType().Name);
                Print(x.LeftOperand, indentation + DefaultIndentation);
                Print(x.RightOperand, indentation + DefaultIndentation);
            }
            else if (e is BinaryMul) {
                BinaryMul x = e as BinaryMul;
                AddLine(new String(' ', indentation) + x.GetType().Name);
                Print(x.LeftOperand, indentation + DefaultIndentation);
                Print(x.RightOperand, indentation + DefaultIndentation);
            }
            else if (e is BinaryDiv) {
                BinaryDiv x = e as BinaryDiv;
                AddLine(new String(' ', indentation) + x.GetType().Name);
                Print(x.LeftOperand, indentation + DefaultIndentation);
                Print(x.RightOperand, indentation + DefaultIndentation);
            }
            else if (e is BinaryExp) {
                BinaryExp x = e as BinaryExp;
                AddLine(new String(' ', indentation) + x.GetType().Name);
                Print(x.LeftOperand, indentation + DefaultIndentation);
                Print(x.RightOperand, indentation + DefaultIndentation);
            }
            else if (e is Literal) {
                Literal x = e as Literal;
                AddLine(new String(' ', indentation) + x.GetType().Name + " (" + x.Value + ")");
            }
            else {
                throw new ArgumentException("Unsupported type: " + e);
            }
        }
    }
}
