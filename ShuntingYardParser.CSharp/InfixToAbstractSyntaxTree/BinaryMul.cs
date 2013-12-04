using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShuntingYardParser.CSharp.InfixToAbstractSyntaxTree {
    public class BinaryMul : Expression {
        public Expression LeftOperand { get; set; }
        public Expression RightOperand { get; set; }

        public override int Evaluate() {
            return LeftOperand.Evaluate() * RightOperand.Evaluate();
        }
    }
}
