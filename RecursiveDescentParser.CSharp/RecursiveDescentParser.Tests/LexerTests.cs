using RecursiveDescentParser.Core;
using Xunit;

namespace RecursiveDescentParser.Tests;

public class LexerTests
{
    [Fact]
    public void Tokens()
    {
        var l = new Lexer("0 0.0 00.01 42 + - * / ^ 3.14 () x");
        AssertToken(l.NextToken(), TokenKind.Integer, "0", 0);
        AssertToken(l.NextToken(), TokenKind.Float, "0.0", 0.0);
        AssertToken(l.NextToken(), TokenKind.Float, "00.01", 0.01);
        AssertToken(l.NextToken(), TokenKind.Integer, "42", 42);
        AssertToken(l.NextToken(), TokenKind.Plus, "+", null);
        AssertToken(l.NextToken(), TokenKind.Minus, "-", null);
        AssertToken(l.NextToken(), TokenKind.Multiplication, "*", null);
        AssertToken(l.NextToken(), TokenKind.Division, "/", null);
        AssertToken(l.NextToken(), TokenKind.Power, "^", null);
        AssertToken(l.NextToken(), TokenKind.Float, "3.14", 3.14);
        AssertToken(l.NextToken(), TokenKind.LParen, "(", null);
        AssertToken(l.NextToken(), TokenKind.RParen, ")", null);            
        AssertToken(l.NextToken(), TokenKind.Illegal, "x", null);
        AssertToken(l.NextToken(), TokenKind.Eof, null, null);
    }

    [Fact]
    public void SkipWhitespace()
    {
        var l = new Lexer("\n\r   42 \t\v");
        AssertToken(l.NextToken(), TokenKind.Integer, "42", 42);
        AssertToken(l.NextToken(), TokenKind.Eof, "", null);
    }

    private void AssertToken(Token token, TokenKind type, string? lexeme, object? literal)
    {
        Assert.Equal(type, token.Kind);
        Assert.Equal(lexeme, token.Lexeme);
        Assert.Equal(literal, token.Literal);
    }
}