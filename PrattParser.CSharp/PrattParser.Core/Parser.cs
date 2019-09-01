using System;
using System.Collections.Generic;

namespace PrattParser.Core
{
    public interface IPrefixParser
    {
	    IExpression Parse(Parser parser, Token token);
    }

    public interface IInfixParser
    {
        int Precedence { get; }
        IExpression Parse(Parser parser, IExpression left, Token token);
    }

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
            return new PrefixExpression
            {
                Token = token,
                Operator = token.Literal, // Should probably be kind
                Right = right
            };
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
            return new InfixExpression
            {
                Token = token,
                Left = left,
                Operator = token.Literal, // Should probably be Kind instead?
                Right = right
            };
        }	
    }

    public class IntegerParser : IPrefixParser
    {
        public IExpression Parse(Parser parser, Token token)
        {
            var literal = new IntegerLiteral { Token = token };
            var ok = long.TryParse(token.Literal, out long value);
            if (!ok)
                throw new Exception($"Couldn't parse '{token.Literal}' as System.Int64");

            literal.Value = value;
            return literal;
        }
    }

    public class FloatParser : IPrefixParser
    {
        public IExpression Parse(Parser parser, Token token)
        {
            var literal = new FloatLiteral { Token = token };
            var ok = double.TryParse(token.Literal, out double value);
            if (!ok)
                throw new Exception($"Couldn't parse '{token.Literal}' as System.Int64");

            literal.Value = value;
            return literal;        
        }
    }

    public class GroupedParser : IPrefixParser
    {
        public int Precedence => throw new NotImplementedException();

        public IExpression Parse(Parser parser, Token token)
        {
            // ConsumeToken();
            // var expression = ParseExpression(PrecedenceLevel.Lowest);

            // if (_peekToken.Kind == TokenKind.RParen)
            // {
            //     ConsumeToken();
            //     return expression;
            // }
            // else
            //     throw new Exception($"Expected next token to be {TokenKind.RParen}, but got {_peekToken.Kind}");
            return null;
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
        public const int Sum  = 10;
        public const int Product = 20;
        public const int Exponent = 30;
        public const int Prefix = 40;
    }

    public class Parser
    {
        Lexer _lexer;
        List<Token> _lookAhead = new List<Token>();

        Dictionary<TokenKind, IPrefixParser> _prefixParsers;
        Dictionary<TokenKind, IInfixParser> _infixParsers;

        private void Register(TokenKind kind, IPrefixParser parser) => _prefixParsers.Add(kind, parser);
        private void Register(TokenKind kind, IInfixParser parser) => _infixParsers.Add(kind, parser);
        private void Prefix(TokenKind kind, int precedence) => Register(kind, new PrefixOperatorParser(precedence));
        private void InfixLeft(TokenKind kind, int precedence) => Register(kind, new InfixOperatorParser(precedence, false));
        private void InfixRight(TokenKind kind, int precedence) => Register(kind, new InfixOperatorParser(precedence, true));

        public Parser(Lexer lexer)
        {
            _lexer = lexer;
            _prefixParsers = new Dictionary<TokenKind, IPrefixParser>();

            // Register the ones that need special parsers
            Register(TokenKind.Integer, new IntegerParser());
            Register(TokenKind.Float, new FloatParser());

            // Register the simple operator parsers
            Prefix(TokenKind.Minus, PrecedenceLevel.Prefix);
            Prefix(TokenKind.LParen, PrecedenceLevel.Prefix);

            _infixParsers = new Dictionary<TokenKind, IInfixParser>();            
            InfixLeft(TokenKind.Plus, PrecedenceLevel.Sum);
            InfixLeft(TokenKind.Minus, PrecedenceLevel.Sum);
            InfixLeft(TokenKind.Slash, PrecedenceLevel.Product);
            InfixLeft(TokenKind.Star, PrecedenceLevel.Product);
            InfixRight(TokenKind.Caret, PrecedenceLevel.Exponent);
        }

        public IExpression Parse() => ParseExpression(PrecedenceLevel.Lowest);

        // The crux of the Pratt parser. Compare to paper.
        public IExpression ParseExpression(int precedence)
        {
	        var token = Consume();
            var ok = _prefixParsers.TryGetValue(token.Kind, out IPrefixParser prefixParser);
            if (!ok)
                throw new Exception($"Couldn't parse '{token.Literal}'");
            var left = prefixParser.Parse(this, token);

            while (precedence < GetPrecedence())
            {
                token = Consume();
                ok = _infixParsers.TryGetValue(token.Kind, out IInfixParser infixParser);
                if (!ok)
                    throw new Exception($"Couldn't parse '{token.Literal}'");
                left = infixParser.Parse(this, left, token);
            }

            return left;
        }

        private int GetPrecedence()
        {
            var ok = _infixParsers.TryGetValue(LookAhead(0).Kind, out IInfixParser parser);
            if (!ok)
                return 0;
            return parser.Precedence;
        }

        private Token Consume()
        {
            var token = LookAhead(0);        
            _lookAhead.RemoveAt(0);
            return token;
        }

        private Token LookAhead(int distance)
        {
            while (distance >= _lookAhead.Count)
                _lookAhead.Add(_lexer.NextToken());
            return _lookAhead[distance]; 
        }
    }
}