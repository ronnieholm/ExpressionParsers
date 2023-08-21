using RecursiveDescentParser.Core;
using Xunit;

namespace RecursiveDescentParser.Tests;

public class LexerTests
{
    [Fact]
    public void Tokens()
    {
        var l = new Lexer("0 0.0 00.01 42 + - * / ^ 3.14 ( ) x");
        AssertToken(l.NextToken(), TokenKind.Integer, new Location(0, 1),"0", 0L);
        AssertToken(l.NextToken(), TokenKind.Float, new Location(2, 5),"0.0", 0.0);
        AssertToken(l.NextToken(), TokenKind.Float, new Location(6, 11),"00.01", 0.01);
        AssertToken(l.NextToken(), TokenKind.Integer, new Location(12, 14),"42", 42L);
        AssertToken(l.NextToken(), TokenKind.Plus, new Location(15, 16),"+", null);
        AssertToken(l.NextToken(), TokenKind.Minus, new Location(17, 18),"-", null);
        AssertToken(l.NextToken(), TokenKind.Multiplication, new Location(19, 20),"*", null);
        AssertToken(l.NextToken(), TokenKind.Division, new Location(21, 22),"/", null);
        AssertToken(l.NextToken(), TokenKind.Power, new Location(23, 24),"^", null);
        AssertToken(l.NextToken(), TokenKind.Float, new Location(25, 29),"3.14", 3.14);
        AssertToken(l.NextToken(), TokenKind.LParen, new Location(30, 31),"(", null);
        AssertToken(l.NextToken(), TokenKind.RParen, new Location(32, 33),")", null);            
        AssertToken(l.NextToken(), TokenKind.Illegal, new Location(34, 35),"x", null);
        AssertToken(l.NextToken(), TokenKind.Eof, new Location(35, 35),null, null);
    }

    [Fact]
    public void SkipWhitespace()
    {
        var l = new Lexer("\n\r   42 \t\v");
        AssertToken(l.NextToken(), TokenKind.Integer, new Location(5, 7), "42", 42);
        AssertToken(l.NextToken(), TokenKind.Eof, new Location(10, 10), null, null);
    }

    private void AssertToken(Token token, TokenKind type, Location location, string? lexeme, object? literal)
    {
        Assert.Equal(type, token.Kind);
        Assert.Equal(location, token.Location);
        Assert.Equal(lexeme, token.Lexeme);
        Assert.Equal(literal, token.Literal);
    }
}