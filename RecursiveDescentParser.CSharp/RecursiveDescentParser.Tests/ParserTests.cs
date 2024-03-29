using Xunit;
using RecursiveDescentParser.Core;

namespace RecursiveDescentParser.Tests;

public class ParserTests
{
    [Theory]
    [InlineData("42", 42)]
    [InlineData("-42", -42)]
    [InlineData("--42", 42)]        
    [InlineData("(42)", 42)]
    [InlineData("((42))", 42)]
    [InlineData("(((42)))", 42)]
    [InlineData("-(42)", -42)]
    public void IntegerLiteral(string input, int expected)
    {
        var l = new Lexer(input);
        var p = new Parser(l, new Tracer());
        var i = p.Parse();
        //Assert.Equal(expected, i);
    }

    [Theory]
    [InlineData("4 + 8 + 3", 15)]
    [InlineData("20 - 7 + 2", 15)]
    [InlineData("20 - 7 - 2", 11)]
    [InlineData("20 / 5 * 2", 8)]
    [InlineData("2 + 3 * 4", 14)]
    [InlineData("(2 + 3) * 4", 20)]
    [InlineData("2^3", 8)]
    [InlineData("2^-3", 0.125)]
    [InlineData("(2^3)^2", 64)]
    [InlineData("2^3^-4", 1.0086)]
    [InlineData("2^3^-0.5", 1.4921)]
    [InlineData("-(2 + 3) * 4", -20)]
    [InlineData("-(-2 + -3) * --4", 20)]
    public void Operators(string input, double expected)
    {
        var l = new Lexer(input);
        var p = new Parser(l, new Tracer());
        var i = p.Parse();
        //Assert.Equal(expected, i, 4);
    }
}

public class AstVisitorTests
{
    [Theory]
    [InlineData("0.5", "0.5", "0.5", "0.5", 0.5)]
    [InlineData("1", "1", "1", "1", 1)]
    [InlineData("(1)", "1", "1", "1", 1)]
    [InlineData("-1", "(-1)", "- 1", "1 -", -1)]
    [InlineData("(((-1)))", "(-1)", "- 1", "1 -", -1)]
    [InlineData("--1", "(-(-1))", "- - 1", "1 - -", 1)]
    [InlineData("1 + 2", "(1 + 2)", "+ 1 2", "1 2 +", 3)]
    [InlineData("1 + 2 + 3", "((1 + 2) + 3)", "+ + 1 2 3", "1 2 + 3 +", 6)]
    [InlineData("1 + 2 - 3", "((1 + 2) - 3)", "- + 1 2 3", "1 2 + 3 -", 0)]
    [InlineData("1 - 2 + 3", "((1 - 2) + 3)", "+ - 1 2 3", "1 2 - 3 +", 2)]
    [InlineData("1 + 2 * 3", "(1 + (2 * 3))", "+ 1 * 2 3", "1 2 3 * +", 7)]
    [InlineData("(1 + 2) * 3", "((1 + 2) * 3)", "* + 1 2 3", "1 2 + 3 *", 9)]
    [InlineData("1 * 2 / 3", "((1 * 2) / 3)", "/ * 1 2 3", "1 2 * 3 /", (double)2 / 3)]
    [InlineData("1 / 2 * 3", "((1 / 2) * 3)", "* / 1 2 3", "1 2 / 3 *", 1.5)]
    [InlineData("1 ^ 2", "(1 ^ 2)", "^ 1 2", "1 2 ^", 1)]
    [InlineData("1 ^ 2 ^ 3", "(1 ^ (2 ^ 3))", "^ 1 ^ 2 3", "1 2 3 ^ ^", 1)]
    [InlineData("(1 ^ 2) ^ 3", "((1 ^ 2) ^ 3)", "^ ^ 1 2 3", "1 2 ^ 3 ^", 1)]
    public void ExpressionTests(string input, string infix, string prefix, string postfix, double value)
    {
        var l = new Lexer(input);
        var p = new Parser(l, new Tracer());
        var e = p.Parse();
        
        var a = new InfixAstFlattener();
        var av = a.Evaluate(e);
        Assert.Equal(infix, av);

        var b = new PrefixAstFlattener();
        var bv = b.Evaluate(e);
        Assert.Equal(prefix, bv);

        var c = new PostfixAstFlattener();
        var cv = c.Evaluate(e);
        Assert.Equal(postfix, cv);

        var d = new Interpreter();
        var dv = d.Evaluate(e);
        Assert.Equal(value, dv);
    }
}