using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Considerations:
//   - The return type of Evaluate() ideally shouldn't be fixed, but generics
//     doesn't allow us to constrain the type parameter to only those types
//     that have arithmetic operators defined.

// Improvements:
//   - Change return type of Evaluate() to floating point to support more
//     accurate computations, e.g., 2^-1 yields 0 with integer math.

namespace ShuntingYardParser.InfixToAbstractSyntaxTree {
    public abstract class Expression {
        public abstract int Evaluate();
    }
}
