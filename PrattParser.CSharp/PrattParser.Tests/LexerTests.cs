using PrattParser.Core;
using Xunit;

namespace PrattParser.Tests
{
    public class LexerTests
    {
        [Fact]
        public void Tokens()
        {
            var l = new Lexer("0 0.0 00.01 42 + - * / ^ 3.14 ()");
            AssertToken(l.NextToken(), TokenKind.Integer, "0");
            AssertToken(l.NextToken(), TokenKind.Float, "0.0");
            AssertToken(l.NextToken(), TokenKind.Float, "00.01");
            AssertToken(l.NextToken(), TokenKind.Integer, "42");
            AssertToken(l.NextToken(), TokenKind.Plus);
            AssertToken(l.NextToken(), TokenKind.Minus);
            AssertToken(l.NextToken(), TokenKind.Star);
            AssertToken(l.NextToken(), TokenKind.Slash);
            AssertToken(l.NextToken(), TokenKind.Caret);
            AssertToken(l.NextToken(), TokenKind.Float, "3.14");
            AssertToken(l.NextToken(), TokenKind.LParen);
            AssertToken(l.NextToken(), TokenKind.RParen);            
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
            Assert.Equal(value, token.Literal);
        }
    }
}
