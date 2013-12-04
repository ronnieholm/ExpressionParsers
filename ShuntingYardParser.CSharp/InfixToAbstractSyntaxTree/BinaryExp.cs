using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShuntingYardParser.InfixToAbstractSyntaxTree {
    public class BinaryExp : Expression {
        public Expression LeftOperand { get; set; }
        public Expression RightOperand { get; set; }

        public override int Evaluate() {
            return (int)Math.Pow(((double)LeftOperand.Evaluate()), ((double)RightOperand.Evaluate()));
        }
    }
}
