using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShuntingYardParser.CSharp.Lexer {
    public enum TokenType {
        None = 0,
        Literal,
        LeftParen,
        RightParen,
        BinaryPlus,
        BinaryMinus,
        BinaryMul,
        BinaryDiv,
        BinaryExp,
        UnaryMinus,
        End
    }
}
