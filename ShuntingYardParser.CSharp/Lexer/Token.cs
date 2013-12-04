using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShuntingYardParser.CSharp.Lexer {
    public class Token {
        public TokenType Type { get; set; }
        public string Lexeme { get; set; }
    }
}
