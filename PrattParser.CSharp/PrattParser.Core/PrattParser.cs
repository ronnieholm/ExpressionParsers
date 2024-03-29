using System;
using System.Collections.Generic;

namespace PrattParser.Core;

public interface IPrefixParser
{
    IExpression Parse(PrattParser parser, Token token);
}

public interface IInfixParser
{
    int Precedence { get; }
    IExpression Parse(PrattParser parser, IExpression left, Token token);
}

public class PrattParser
{
    private readonly Lexer _lexer;
    private readonly List<Token> _lookAhead = new();

    private readonly Dictionary<TokenKind, IPrefixParser> _prefixParsers;
    private readonly Dictionary<TokenKind, IInfixParser> _infixParsers;

    protected PrattParser(Lexer lexer)
    {
        _lexer = lexer;
        _prefixParsers = new Dictionary<TokenKind, IPrefixParser>();
        _infixParsers = new Dictionary<TokenKind, IInfixParser>();
    }

    protected void Register(TokenKind kind, IPrefixParser parser) => _prefixParsers.Add(kind, parser);
    protected void Register(TokenKind kind, IInfixParser parser) => _infixParsers.Add(kind, parser);

    public IExpression Parse()
    {
        var expression = ParseExpression(PrecedenceLevel.Lowest);
        var current = LookAhead(0);
        if (current.Kind != TokenKind.Eof)
            throw new Exception($"Expected {TokenKind.Eof} but got {current.Kind} of '{current.Literal}'");
        return expression;
    }

    // The crux of the Pratt parser. Compare to the Pratt paper.
    public IExpression ParseExpression(int precedence)
    {
        var token = Consume();
        var ok = _prefixParsers.TryGetValue(token.Kind, out var prefixParser);
        if (!ok)
            throw new Exception($"Couldn't parse '{token.Literal}'");
        var left = prefixParser!.Parse(this, token);

        while (precedence < GetPrecedence())
        {
            token = Consume();
            ok = _infixParsers.TryGetValue(token.Kind, out var infixParser);
            if (!ok)
                throw new Exception($"Couldn't parse '{token.Literal}'");
            left = infixParser!.Parse(this, left, token);
        }

        return left;
    }

    private int GetPrecedence()
    {
        var ok = _infixParsers.TryGetValue(LookAhead(0).Kind, out var parser);
        return !ok ? 0 : parser!.Precedence;
    }

    public Token Consume(TokenKind kind)
    {
        var token = LookAhead(0);
        if (token.Kind != kind)
            throw new Exception($"Expected token '{kind}' but found '{token.Kind}'");
        return Consume();
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