using ShuntingYardParser.InfixToAbstractSyntaxTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShuntingYardParser.InfixToAbstractSyntaxTree {
    public class UnaryMinus : Expression {
        public Expression Operand { get; set; }

        public override int Evaluate() {
            return -Operand.Evaluate();
        }
    }
}
