using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShuntingYardParser.CSharp.InfixToAbstractSyntaxTree {
    public class Literal : Expression {
        public int Value { get; set; }

        public override int Evaluate() {
            return Value;
        }
    }
}
