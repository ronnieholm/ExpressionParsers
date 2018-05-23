using System;
using RecursiveDescentParser.Core;
using Xunit;

namespace RecursiveDescentParser.Tests
{
    public class LexerTests
    {
        [Fact]
        public void Tokens()
        {
            var l = new Lexer("0 0.0 42 + - * / ^ 3.14");
            AssertToken(l.NextToken(), TokenKind.Integer, "0");
            AssertToken(l.NextToken(), TokenKind.Float, "0");
            AssertToken(l.NextToken(), TokenKind.Integer, "42");
            AssertToken(l.NextToken(), TokenKind.Plus);
            AssertToken(l.NextToken(), TokenKind.Minus);
            AssertToken(l.NextToken(), TokenKind.Multiplication);
            AssertToken(l.NextToken(), TokenKind.Division);
            AssertToken(l.NextToken(), TokenKind.Power);
            AssertToken(l.NextToken(), TokenKind.Float, "3.14");
            AssertToken(l.NextToken(), TokenKind.Eof);
        }       

        [Fact]
        public void SkipWhitespace()
        {
            var l = new Lexer("\n\r   42 \t\v");
            AssertToken(l.NextToken(), TokenKind.Integer, "42");
            AssertToken(l.NextToken(), TokenKind.Eof);
        }

        private void AssertToken(Token token, TokenKind type)
        {
            Assert.Equal(type, token.Kind);            
        }

        private void AssertToken(Token token, TokenKind type, string value)
        {
            Assert.Equal(type, token.Kind);
            Assert.Equal(value, token.Value);
        }
    }
}
