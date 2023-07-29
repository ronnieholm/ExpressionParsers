using System;

namespace PrattParser.Core;

public enum TokenKind
{
    // Rather than terminate lexing on an unknown character, we return a special
    // Illegal token kind.
    Illegal,
    Eof,
    Integer,
    Float,
    Plus,
    Minus,
    Star,
    Slash,        
    Caret,
    LParen,
    RParen,
    Bang
}

// Improvement: extend ReportSyntaxError in lexer and parser with visual
// indicators of error position in source text.
public record Token(TokenKind Kind, string Literal);

public class Lexer
{
    readonly string _input;
    int _currentPosition;

    private char CurrentCharacter => _currentPosition < _input.Length ? _input[_currentPosition] : '\0';

    public Lexer(string input)
    {
        // No lexer state to initialize except for input because
        // _currentPosition is already initialized to zero and CurrentCharacter
        // is computed based on _currentPosition.
        _input = input;
    }

    public Token NextToken()
    {
        // Alternative: we could've started off with 
        //
        // while (char.IsWhiteSpace(GetCurrentCharacter())) 
        // {
        //     _currentPosition++;
        // }
        //
        // to consume leading whitespace. Instead we handle whitespace in the
        // switch statement using a goto as this provides uniform treatment of
        // characters without biasing the lexer toward whitespace. For each
        // character, the while expression would be evaluated which, depending
        // on the language being lexed, may be inefficient.
    retry:
        switch (CurrentCharacter)
        {
            case '\0':
                return new Token(TokenKind.Eof, "");
            case '0':
            case '1':
            case '2':
            case '3':
            case '4':
            case '5':
            case '6':
            case '7':
            case '8':
            case '9':
                // Once we know whether we're lexing a float or an integer,
                // we'll backtrack using the bookmark and call the appropriate
                // lexer method. Note that LL(1) refers to one token lookahead
                // at the parser level, not the lexer. A lot of languages that
                // are LL(1) at the token level aren't LL(1) at the character
                // level. One reason to separate the lexer and parser is to
                // allow the parser to be LL(1).
                var bookmark = _currentPosition;
                while (char.IsDigit(CurrentCharacter))
                {
                    _currentPosition++;
                }
                if (CurrentCharacter == '.')
                {
                    _currentPosition = bookmark;
                    var floatString = LexFloat();
                    return new Token(TokenKind.Float, floatString);                       
                }
                else
                {
                    _currentPosition = bookmark;
                    var integerString = LexInteger();
                    return new Token(TokenKind.Integer, integerString);    
                }
            case '+':
                _currentPosition++;
                return new Token(TokenKind.Plus, "+");
            case '-':
                _currentPosition++;
                return new Token(TokenKind.Minus, "-");                
            case '*':
                _currentPosition++;
                return new Token(TokenKind.Star, "*");                
            case '/':
                _currentPosition++;
                return new Token(TokenKind.Slash, "/");            
            case '^':
                _currentPosition++;
                return new Token(TokenKind.Caret, "^");            
            case '(':
                _currentPosition++;
                return new Token(TokenKind.LParen, "(");                
            case ')':
                _currentPosition++;
                return new Token(TokenKind.RParen, ")");
            case '!':
                _currentPosition++;
                return new Token(TokenKind.Bang, "!");                
            case ' ':
            case '\n':
            case '\r':
            case '\t':
            case '\v':
                _currentPosition++;
                goto retry;
            default:
                return new Token(TokenKind.Illegal, CurrentCharacter.ToString());
        }
    }

    // Integer = Digit | Integer Digit
    private string LexInteger()
    {
        // Don't parse by returning an Int32 or Int64. Instead return the
        // integer as a string and leave it to the parser to intepret it. It may
        // be the integer is too large for the Int32 or Int64.
        var start = _currentPosition++;
        while (char.IsDigit(CurrentCharacter))
        {
            _currentPosition++;
        }
        return _input.Substring(start, _currentPosition - start);
    }

    // Float = Integer "." Integer
    private string LexFloat()
    {
        var start = _currentPosition;
        LexInteger();
        _currentPosition++;
        if (!char.IsDigit(CurrentCharacter))
        {
            ReportSyntaxError("digit");
        }
        LexInteger();
        var end = _currentPosition;
        return _input.Substring(start, end - start);
    }

    private void ReportSyntaxError(string expected)
    {
        var character = CurrentCharacter == '\0' ? "Eof" : CurrentCharacter.ToString();
        throw new Exception($"Expected {expected}. Got '{character}'");
    }
}