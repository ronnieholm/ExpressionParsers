using Xunit;
using PrattParser.Core;

namespace PrattParser.Tests
{
    public class ParserTests
    {
        [Theory]
        [InlineData("42", "42i")]
        [InlineData("-42", "(-42i)")]
        [InlineData("--42", "(-(-42i))")]        
        [InlineData("(42)", "42i")]
        [InlineData("((42))", "42i")]
        [InlineData("(((42)))", "42i")]
        [InlineData("-(42)", "(-42i)")]
        public void IntegerLiteral(string input, string expected)
        {
            var l = new Lexer(input);
            var p = new Parser(l);
            var i = p.Parse();
            Assert.Equal(expected, i.String);
        }

        [Theory]
        [InlineData("4 + 8 + 3", 15)]
        // [InlineData("20 - 7 + 2", 15)]
        // [InlineData("20 - 7 - 2", 11)]
        // [InlineData("20 / 5 * 2", 8)]
        // [InlineData("2 + 3 * 4", 14)]
        // [InlineData("(2 + 3) * 4", 20)]
        // [InlineData("2^3", 8)]
        // [InlineData("2^-3", 0.125)]
        // [InlineData("(2^3)^2", 64)]
        // [InlineData("2^3^-4", 1.0086)]
        // [InlineData("2^3^-0.5", 1.4921)]
        // [InlineData("-(2 + 3) * 4", -20)]
        // [InlineData("-(-2 + -3) * --4", 20)]
        public void Operators(string input, double expected)
        {
            var l = new Lexer(input);
            var p = new Parser(l);
            var i = p.Parse();
            //Assert.Equal(expected, i, 4);
        }
    }
}
