namespace ShuntingYardParser.CSharp.Lexer;

public enum TokenType
{
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