using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShuntingYardParser.Lexer {
    public enum TokenType {
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
