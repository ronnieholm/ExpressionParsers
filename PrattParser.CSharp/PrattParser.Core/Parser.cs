using System;
using System.Collections.Generic;

namespace PrattParser.Core
{
    using PrefixParseFn = Func<IExpression>;
    using InfixParseFn = Func<IExpression, IExpression>;

    public class Parser
    {
        Lexer _lexer;

        // Acts like _currentPosition within lexer, but instead of pointing to a
        // character in the input these point to the current and next tokens. We
        // need to look at _currentToken to decide what to do next, and we need
        // _peekToken to guide the decision in case _currentToken doesn't
        // provide us with enough information. In effect, this implements a
        // parser with one token lookahead.
        Token _currentToken;
        Token _peekToken;

        // Functions based on token type called as part of Pratt parsing.
        Dictionary<TokenKind, PrefixParseFn> _prefixParseFns;
        Dictionary<TokenKind, InfixParseFn> _infixParseFns;

        // Actual numeric numbers doesn't matter, but the order and the relation
        // to each other does. We want to be able to answer questions such as
        // whether operator * has higher precedence than operator ==. While
        // using an enum over a class with integer constants alliviates the need
        // to explicitly assign a value to each member, it making debugging the
        // Pratt parser slightly more difficult. During precedence value
        // comparisons, the debugger will show the strings over their its
        // implicit number.
        enum PrecedenceLevel
        {
            None = 0,
            Lowest,
            Sum,
            Product,
            Prefix
        }

        // Table of precedence associated with token. Observe how not every
        // precedence value is present (Lowest and Prefix missing). Lowest
        // serves as a starting precedence for the Pratt parser while Prefix
        // isn't associated with a token but an expression as a whole. On the
        // other hand, some operators, such as multiplication and division,
        // share the same precedence level.
        Dictionary<TokenKind, PrecedenceLevel> precedences = new Dictionary<TokenKind, PrecedenceLevel>
        {
            { TokenKind.Plus, PrecedenceLevel.Sum },
            { TokenKind.Minus, PrecedenceLevel.Sum },
            { TokenKind.Slash, PrecedenceLevel.Product },
            { TokenKind.Star, PrecedenceLevel.Product },
        };

        private void RegisterPrefix(TokenKind t, PrefixParseFn fn) => _prefixParseFns.Add(t, fn);
        private void RegisterInfix(TokenKind t, InfixParseFn fn) => _infixParseFns.Add(t, fn);

        public Parser(Lexer lexer)
        {
            _lexer = lexer;

            _prefixParseFns = new Dictionary<TokenKind, PrefixParseFn>();
            RegisterPrefix(TokenKind.Integer, ParseIntegerLiteral);
            RegisterPrefix(TokenKind.Float, ParseFloatLiteral);
            RegisterPrefix(TokenKind.Minus, ParsePrefixExpression);
            RegisterPrefix(TokenKind.LParen, ParseGroupedExpression);

            _infixParseFns = new Dictionary<TokenKind, InfixParseFn>();
            RegisterInfix(TokenKind.Plus, ParseInfixExpression);
            RegisterInfix(TokenKind.Minus, ParseInfixExpression);
            RegisterInfix(TokenKind.Slash, ParseInfixExpression);
            RegisterInfix(TokenKind.Star, ParseInfixExpression);

            // Read two tokens to set both _currentToken and _peekToken.
            NextToken();
            NextToken();
        }

        public IExpression Parse() => ParseExpression(PrecedenceLevel.Lowest);

        // The crux of the Pratt parser. Compare to paper.
        private IExpression ParseExpression(PrecedenceLevel precedence)
        {
            PrefixParseFn prefixFn;
            var ok = _prefixParseFns.TryGetValue(_currentToken.Kind, out prefixFn);
            if (!ok)
                throw new Exception($"No prefix parse function for {_currentToken.Kind} found");

            var leftExpression = prefixFn();

            // precedence is what the original Pratt paper refers to as
            // right-binding power and the return of peekPrecedenceLevel
            // what's referred to as left-binding power. For as long as
            // left-binding power > right-binding power, another level gets
            // added to the Abstract Syntax Tree. Another levels corresponds to
            // operations which need to be carried out first when the expression
            // is evaluated.
            var ok1 = precedences.TryGetValue(_peekToken.Kind, out PrecedenceLevel level);

            // Returning Lowest when precedence cannot be found for a token is
            // what enables us to parse for instance grouped expression. The
            // RParen doesn't have an associated precedence, so when Lowest is
            // returned it causes the parser to finish evaluating a
            // subexpression as a whole.
            var peekPrecedenceLevel = ok1 ? level : PrecedenceLevel.Lowest;

            while (_peekToken.Kind != TokenKind.Eof && precedence < peekPrecedenceLevel)
            {
                InfixParseFn infixFn;
                ok = _infixParseFns.TryGetValue(_peekToken.Kind, out infixFn);
                if (!ok)
                    return leftExpression;

                NextToken();
                leftExpression = infixFn(leftExpression);
            }

            return leftExpression;
        }

        private IExpression ParseIntegerLiteral()
        {
            var literal = new IntegerLiteral { Token = _currentToken };

            long value;
            var ok = long.TryParse(_currentToken.Literal, out value);
            if (!ok)
                throw new Exception($"Could not parse '{_currentToken.Literal}' as System.Int64");

            literal.Value = value;
            return literal;
        }

        private IExpression ParseFloatLiteral()
        {
            var literal = new FloatLiteral { Token = _currentToken };

            double value;
            var ok = double.TryParse(_currentToken.Literal, out value);
            if (!ok)
                throw new Exception($"Could not parse '{_currentToken.Literal}' as System.Double");

            literal.Value = value;
            return literal;
        }

        private IExpression ParseGroupedExpression()
        {
            NextToken();
            var expression = ParseExpression(PrecedenceLevel.Lowest);

            if (_peekToken.Kind == TokenKind.RParen)
            {
                NextToken();
                return expression;
            }
            else
                throw new Exception($"Expected next token to be {TokenKind.RParen}, got {_peekToken.Kind} instead.");
        }

        private IExpression ParsePrefixExpression()
        {
            var expression = new PrefixExpression { Token = _currentToken, Operator = _currentToken.Literal };
            NextToken();
            expression.Right = ParseExpression(PrecedenceLevel.Prefix);
            return expression;
        }

        private IExpression ParseInfixExpression(IExpression left)
        {
            var expression = new InfixExpression { Token = _currentToken, Operator = _currentToken.Literal, Left = left };            
            NextToken();
            var ok = precedences.TryGetValue(_currentToken.Kind, out PrecedenceLevel precedenceLevel);
            var level = ok ? precedenceLevel : PrecedenceLevel.Lowest;
            expression.Right = ParseExpression(level);
            return expression;
        }

        private void NextToken()
        {
            _currentToken = _peekToken;
            _peekToken = _lexer.NextToken();
        }
    }
}