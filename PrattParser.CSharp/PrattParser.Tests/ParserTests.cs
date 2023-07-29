using Xunit;
using PrattParser.Core;

namespace PrattParser.Tests;

public class ParserTests
{
    [Theory]
    [InlineData("42", "42")]
    [InlineData("-42", "(-42)")]
    [InlineData("--42", "(-(-42))")]        
    [InlineData("(42)", "42")]
    [InlineData("((42))", "42")]
    [InlineData("(((42)))", "42")]
    [InlineData("-(42)", "(-42)")]
    public void IntegerLiteral(string input, string expected)
    {
        var l = new Lexer(input);
        var p = new ExpressionParser(l);
        var i = p.Parse();
        Assert.Equal(expected, i.String);
    }

    [Theory]
    [InlineData("4 + 8 + 3", "((4 + 8) + 3)")]
    [InlineData("20 - 7 + 2", "((20 - 7) + 2)")]
    [InlineData("20 - 7 - 2", "((20 - 7) - 2)")]
    [InlineData("20 / 5 * 2", "((20 / 5) * 2)")]
    [InlineData("2 + 3 * 4", "(2 + (3 * 4))")]
    [InlineData("(2 + 3) * 4", "((2 + 3) * 4)")]
    [InlineData("2^3", "(2 ^ 3)")]
    [InlineData("2^-3", "(2 ^ (-3))")]
    [InlineData("(2^3)^2", "((2 ^ 3) ^ 2)")]
    [InlineData("2^3^-4", "(2 ^ (3 ^ (-4)))")]
    [InlineData("2^3^-0.5", "(2 ^ (3 ^ (-0.5)))")]
    [InlineData("-(2 + 3) * 4", "(-((2 + 3) * 4))")]
    [InlineData("-(-2 + -3) * --4", "(-((-(2 + (-3))) * (-(-4))))")]
    [InlineData("-2!", "(-(2!))")]
    public void Operators(string input, string expected)
    {
        var l = new Lexer(input);
        var p = new ExpressionParser(l);
        var i = p.Parse();
        Assert.Equal(expected, i.String);
    }
}
