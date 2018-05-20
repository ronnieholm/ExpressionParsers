using System;
using RecursiveDescentParser.Core;
using Xunit;

namespace RecursiveDescentParser.Tests
{
    public class LexerTests
    {
        [Fact]
        public void IntegerLiteral()
        {
            var l = new Lexer("42+-*/^");
            TestToken(l.NextToken(), TokenKind.Integer, "42");
            TestToken(l.NextToken(), TokenKind.Plus);
            TestToken(l.NextToken(), TokenKind.Minus);
            TestToken(l.NextToken(), TokenKind.Multiplication);
            TestToken(l.NextToken(), TokenKind.Division);
            TestToken(l.NextToken(), TokenKind.Power);
            TestToken(l.NextToken(), TokenKind.Eof);
        }

        [Fact]
        public void AddExpression()
        {
            var l = new Lexer("42 + 64");
            TestToken(l.NextToken(), TokenKind.Integer, "42");
            TestToken(l.NextToken(), TokenKind.Plus);
            TestToken(l.NextToken(), TokenKind.Integer, "64");
            TestToken(l.NextToken(), TokenKind.Eof);
        }

        [Fact]
        public void SkipWhitespace()
        {
            var l = new Lexer("\n\r   42 \t\v");
            TestToken(l.NextToken(), TokenKind.Integer, "42");
            TestToken(l.NextToken(), TokenKind.Eof);
        }

        private void TestToken(Token token, TokenKind type)
        {
            Assert.Equal(type, token.Kind);            
        }

        private void TestToken(Token token, TokenKind type, string value)
        {
            Assert.Equal(type, token.Kind);
            Assert.Equal(value, token.Value);
        }
    }
}
