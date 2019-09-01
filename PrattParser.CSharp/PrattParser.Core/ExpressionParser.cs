using System;

namespace PrattParser.Core
{
    public class PrefixOperatorParser : IPrefixParser
    {
        public int Precedence { get; }

        public PrefixOperatorParser(int precedence)
        {
            Precedence = precedence;
        }

        public IExpression Parse(Parser parser, Token token)
        {
            var right = parser.ParseExpression(PrecedenceLevel.Lowest);
            return new PrefixExpression(token, token.Kind, right);
        }
    }

    public class InfixOperatorParser : IInfixParser
    {
        public int Precedence { get; }
        public bool IsRight { get; }

        public InfixOperatorParser(int precedence, bool isRight)
        {
            Precedence = precedence;
            IsRight = isRight;
        }

        public IExpression Parse(Parser parser, IExpression left, Token token)
        {
            var right = parser.ParseExpression(Precedence - (IsRight ? 1 : 0));
            return new InfixExpression(token, left, token.Kind, right);
        }	
    }

    public class PostfixOperatorParser : IInfixParser
    {
        public int Precedence { get; }

        public PostfixOperatorParser(int precedence)
        {
            Precedence = precedence;
        }

        public IExpression Parse(Parser parser, IExpression left, Token token)
        {
            return new PostfixExpression(token, token.Kind, left);
        }
    }

    public class IntegerParser : IPrefixParser
    {
        public IExpression Parse(Parser parser, Token token)
        {
            var ok = long.TryParse(token.Literal, out long value);
            if (!ok)
                throw new Exception($"Couldn't parse '{token.Literal}' as System.Int64");
            return new IntegerLiteral(token, value);
        }
    }

    public class FloatParser : IPrefixParser
    {
        public IExpression Parse(Parser parser, Token token)
        {
            var ok = double.TryParse(token.Literal, out double value);
            if (!ok)
                throw new Exception($"Couldn't parse '{token.Literal}' as System.Int64");
            return new FloatLiteral(token, value);
        }
    }

    public class GroupParser : IPrefixParser
    {
        public IExpression Parse(Parser parser, Token token)
        {
            var expression = parser.ParseExpression(PrecedenceLevel.Lowest);
            parser.Consume(TokenKind.RParen);
            return expression;
        }
    }

    // Actual numeric numbers doesn't matter, but the order and the relation
    // to each other does. We want to be able to answer questions such as
    // whether operator * has higher precedence than operator ==. While
    // using an enum over a class with integer constants alliviates the need
    // to explicitly assign a value to each member, it making debugging the
    // Pratt parser slightly more difficult. During precedence value
    // comparisons, the debugger will show the strings over their its
    // implicit number.
    public class PrecedenceLevel
    {
        public const int Lowest = 0;
        public const int Sum  = 1;
        public const int Product = 2;
        public const int Exponent = 3;
        public const int Prefix = 4;
        public const int Postfix = 5;
    }

    public class ExpressionParser : Parser
    {
        public ExpressionParser(Lexer lexer) : base(lexer)
        {
            // Register the ones that need special parsers
            Register(TokenKind.Integer, new IntegerParser());
            Register(TokenKind.Float, new FloatParser());
            Register(TokenKind.LParen, new GroupParser());

            // Register the simple operator parsers
            Prefix(TokenKind.Minus, PrecedenceLevel.Prefix);

            InfixLeft(TokenKind.Plus, PrecedenceLevel.Sum);
            InfixLeft(TokenKind.Minus, PrecedenceLevel.Sum);
            InfixLeft(TokenKind.Slash, PrecedenceLevel.Product);
            InfixLeft(TokenKind.Star, PrecedenceLevel.Product);
            InfixRight(TokenKind.Caret, PrecedenceLevel.Exponent);

            Postfix(TokenKind.Bang, PrecedenceLevel.Postfix);                      
        }        

        private void Prefix(TokenKind kind, int precedence) => Register(kind, new PrefixOperatorParser(precedence));
        private void InfixLeft(TokenKind kind, int precedence) => Register(kind, new InfixOperatorParser(precedence, false));
        private void InfixRight(TokenKind kind, int precedence) => Register(kind, new InfixOperatorParser(precedence, true));
        private void Postfix(TokenKind kind, int precedence) => Register(kind, new PostfixOperatorParser(precedence));
    }
}