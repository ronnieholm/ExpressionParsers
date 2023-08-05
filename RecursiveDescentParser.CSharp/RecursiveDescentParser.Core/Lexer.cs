using System;

namespace RecursiveDescentParser.Core;

public enum TokenKind
{
    // Alternative: to cut down on boilerplate, enumeration values could start
    // at 128 rather than 0. This way any single character ASCII token could be
    // encoded as itself. In the lexer's switch statement, we could match these
    // tokens like we do with numbers but with a simpler return.
    //
    // Printing error messages from within the parser, in the PrintToken method,
    // we'd have to check if kind < 128 and if kind is printable and convert it
    // to a char. For kinds < 128, the error message would read "Expected token:
    // '('". With explicit tokens as below it reads "Expected token: LParen".

    // Rather than terminate lexing on an unknown character, we return a
    // special Illegal kind.
    Illegal,
    Eof,
    Integer,
    Float,
    Plus,
    Minus,
    Multiplication,
    Division,
    Power,
    LParen,
    RParen
}

// A typical approach to lexing is to have the Token class hold a TokenType
// enum, the string matched (the lexeme), and start and end positions into the
// input for error reporting. But because an integer or float matched isn't a
// string (and C# doesn't support C style unions), the integer and float is
// converted to a string and stored inside the Token. Later the parser converts
// the string to its actual type based on TokenKind. While minor, these
// conversions are redundant.
//
// A couple of alternatives exist to mitigate the redundant conversations, but
// unless performance is critical, they make the code harder to read and are
// best avoided:
//
// 1. Return only TokenKind, possibly renamed to Token, from NextToken().
//    Calling code is then responsible for inspecting the TokenKind and getting
//    the value from a property on the lexer. The lexer would expose properties
//    for string, integer, float, and so on, and only the relevant property
//    would contain a value. This way, a token of any kind can be returned and
//    accessed without conversion. On the downside, the client may read the
//    wrong property, and it means more state management for the lexer.
//
// 2. Define a generic Token<T> type where T is the type matched, such as
//    string, int, float, ... But then what to do for tokens with no associated
//    type, such and a + og -? Should we set T to string and ignore the matched
//    string? Or set the matched string to "+"?
//
// 3. Define specializations of Token. But since NextToken() returns Token,
//    clients would need to dispatch based on type of Token to access the
//    matched value.
//
// The lexer/parser employs the string approach and converts integers, and
// floats to strings and back again.
public class Token
{
    // Improvement: save lexeme start and end positions. In principle, with
    // start end end, we can infer the token value as a substring of the source
    // and don't need to store it. We pass it anyway since sometimes the string
    // value differs from the source text. For instance, a float in the source
    // may be 3,14 (with comma) whereas its Value is "3.14".
    //
    // Improvement: extend ReportSyntaxError in lexer and parser with visual
    // indicators of error position in source text.
    public TokenKind Kind { get; }
    public string Value { get; }

    public Token(TokenKind type, string value = "")
    {
        Kind = type;
        Value = value;
    }
}

public class Lexer
{
    private readonly string _input;
    private int _currentPos;

    private char CurrentCharacter => _currentPos < _input.Length ? _input[_currentPos] : '\0';

    public Lexer(string input)
    {
        // No lexer state to initialize except for input because _currentPos is
        // already default initialized to zero and _currentChar is computed
        // based on _currentPos.
        _input = input;
    }

    public Token NextToken()
    {
        // Alternative: we could've started off with 
        //
        // while (char.IsWhiteSpace(GetCurrentCharacter())) 
        // {
        //     _currentPos++;
        // }
        //
        // to consume leading whitespace. Instead we handle whitespace in the
        // switch statement using a goto statement. This provides a uniform
        // treatment of characters. Consuming whitespace like above biases the
        // lexer toward whitespace. For each character, the while expression is
        // evaluated which, depending on the language being lexed, may be
        // inefficient.
        retry:
        switch (CurrentCharacter)
        {
            case '\0':
                return new Token(TokenKind.Eof);
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
                // Once we know whether we're at a float or an integer, we'll
                // backtrack using the bookmark and call the appropriate lexer
                // method. Note that LL(1) refers to one token lookahead at the
                // parser level, not the lexer. A lot of languages that are
                // LL(1) at the token level aren't LL(1) at the character level.
                // One reason to separate the lexer and parser is to allow the
                // parser to be LL(1).
                var bookmark = _currentPos;
                while (char.IsDigit(CurrentCharacter))
                {
                    _currentPos++;
                }
                if (CurrentCharacter == '.')
                {
                    _currentPos = bookmark;
                    var float_ = LexFloat();
                    return new Token(TokenKind.Float, float_.ToString());                       
                }
                else
                {
                    _currentPos = bookmark;
                    var integer = LexInteger();
                    return new Token(TokenKind.Integer, integer.ToString());    
                }
            case '+':
                _currentPos++;
                return new Token(TokenKind.Plus);
            case '-':
                _currentPos++;
                return new Token(TokenKind.Minus);                
            case '*':
                _currentPos++;
                return new Token(TokenKind.Multiplication);                
            case '/':
                _currentPos++;
                return new Token(TokenKind.Division);            
            case '^':
                _currentPos++;
                return new Token(TokenKind.Power);            
            case '(':
                _currentPos++;
                return new Token(TokenKind.LParen);                
            case ')':
                _currentPos++;
                return new Token(TokenKind.RParen);
            case ' ':
            case '\n':
            case '\r':
            case '\t':
            case '\v':
                _currentPos++;
                goto retry;
            default:
                return new Token(TokenKind.Illegal, CurrentCharacter.ToString());
        }
    }

    // Integer = Digit | Integer Digit
    private string LexInteger()
    {
        // Don't parse by returning a Int32 or Int64. Instead return the integer
        // as a string and leave it to the parser to interpret it. It may be the
        // integer is too large for the Int32 or Int64.
        var start = _currentPos++;
        while (char.IsDigit(CurrentCharacter))
        {
            _currentPos++;
        }
        return _input.Substring(start, _currentPos - start);
    }

    // Float = Integer "." Integer
    private string LexFloat()
    {
        var start = _currentPos;
        LexInteger();
        _currentPos++;
        if (!char.IsDigit(CurrentCharacter))
        {
            ReportSyntaxError("digit");
        }
        LexInteger();
        var end = _currentPos;
        return _input.Substring(start, end - start);
    }

    private void ReportSyntaxError(string expected)
    {
        var character = CurrentCharacter == '\0' ? "Eof" : CurrentCharacter.ToString();
        throw new Exception($"Expected {expected}. Got '{character}'");
    }
}