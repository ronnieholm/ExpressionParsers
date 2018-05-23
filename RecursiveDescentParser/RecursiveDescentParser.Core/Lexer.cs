using System;

namespace RecursiveDescentParser.Core
{
    public enum TokenKind
    {
        // Alternative: to cut down on boilerplate, enumeration values could
        // start at 128 rather than 0. This way any single charactor ASCII token
        // could be encoded as itself. In the lexer's switch statement, we could
        // match these tokens like we do with numbers but with a simpler, single
        // return.
        //
        // Printing error messages from within the parser, in a PrintToken
        // method, we'd have to check if kind < 128 and printable and convert it
        // to a char. For kinds < 128, the error message would read "Expected
        // token: '('". With expclit tokens as below it reads "Expected token:
        // LParen".

        // Rather than terminate lexing on an unknown character, return a
        // special kind.
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

    // A typical approach to lexing is to have a Token class hold a TokenType
    // enum, the string matched (the lexeme), and start and end positions into
    // the input for error reporting. But because an integer or float matched
    // isn't a string (and C# doesn't support C style unions), the integer or
    // float would have to be stored in a string inside the Token. When
    // required, the parser would convert the string to its actual type based on
    // TokenKind. While minor, these two conversions are redundant.
    //
    // A couple of alternatives exist to mitigate, but unless performance is
    // critical, they make the code harder to read and should be avoided:
    //
    // 1. Return only TokenKind, possibly renamed to Token, from ReadToken().
    //    Calling code is then responsible for inspecting the TokenKind and
    //    getting the value from a property on the lexer. The lexer would expose
    //    properties for string, integer, float, and so on. This way, a token of
    //    any kind can be returned and accessed without conversion. On the
    //    downside, it possible for the client to read the wrong property, and
    //    it means more state management for the lexer.
    //
    // 2. Define a generic Token<T> type where T is the type matched, such as
    //    string, int, float, ... But then what to do for tokens with no
    //    associated type, such and a + og -? Should we set T to string and
    //    ignore the matched string? Or set the matched string to "+"?
    //
    // 3. Define specializations of Token. But since ReadToken() returns Token,
    //    clients would need to dispatch based on type of Token to access the
    //    matched value.
    //
    // This parser employs the string approach and converts integers, and floats
    // to strings.
    public class Token
    {
        // TODO: Save Start and End position, too. In principle, with Start end
        // End, we can infer value and don't need to store it. We pass it anyway
        // since some computation might have been involved with constructing it
        // (such as with integers and floats). In some cases the string value
        // may differ from the source text. For instance, a float in the source
        // may be 3,14 (with comma) whereas its Value is "3.14."
        //
        // TODO: Extend SyntaxError in lexer and parser with visual indicators
        // of error position in source text.
        public TokenKind Kind { get; private set; }
        public string Value { get; private set; }

        public Token(TokenKind type)
        {
            Kind = type;
        }

        public Token(TokenKind type, string value)
        {
            Kind = type;
            Value = value;
        }
    }

    public class Lexer
    {
        string _input;
        int _currentPos;

        public Lexer(string input)
        {
            _input = input;

            // No lexer state to initialize except for input because _currentPos
            // is already default initialized to zero and _currentChar is
            // computed based on _currentPos.
        }

        public Token NextToken()
        {
            // Alternative: we couldn've started off with 
            //
            // while (char.IsWhiteSpace(GetCurrentCharacter())) 
            // {
            //     _currentPos++;
            // }
            //
            // to consume leading whitespace. Instead we handle whitespace in
            // the switch statement using a goto statement. This provides a
            // uniform treatment of characters. Consuming whitespace like above
            // biases the lexer toward whitespace. For each character the while
            // expression is evaluated which, depending on the language being
            // lexed, may be inefficient.
        retry:
            switch (GetCurrentCharacter())
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
                    // Once we know whether we're at a float or integer, we'll
                    // backtrack using the bookmark and call the appropriate
                    // lexer method. Note that LL(1) refers to one token
                    // lookahead at the parser level, not the lexer. A lot of
                    // languages that are LL(1) at the token level aren't LL(1)
                    // at the character level. To allow the parser to be LL(1)
                    // is one reason for seperating the lexer and parser.
                    var bookmark = _currentPos;
                    while (char.IsDigit(GetCurrentCharacter()))
                    {
                        _currentPos++;
                    }
                    if (GetCurrentCharacter() == '.')
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
                    return new Token(TokenKind.Illegal, GetCurrentCharacter().ToString());
            }
        }

        // Integer = Digit | Integer Digit
        private int LexInteger()
        {
            var integer = GetCurrentCharacter() - '0';
            _currentPos++;
            while (char.IsDigit(GetCurrentCharacter()))
            {
                integer *= 10;
                integer += GetCurrentCharacter() - '0';
                _currentPos++;
            }
            return integer;
        }

        // Float = Integer "." Integer
        private float LexFloat()
        {
            var start = _currentPos;
            while (char.IsDigit(GetCurrentCharacter()))
            {
                _currentPos++;
            }
            _currentPos++;
            if (!char.IsDigit(GetCurrentCharacter()))
            {
                ReportSyntaxError("digit");
            }
            while (char.IsDigit(GetCurrentCharacter()))
            {
                _currentPos++;
            }
            var end = _currentPos;

            // The reason why we have two while loops, one for the
            // characteristic and one for the mantissa, instead of calls to
            // LexInteger is that the mantisse could be a zero-prefixed number.
            // In that case the integer returned wouldn't be correct. Instead we
            // locate the start and end position of the float.
            var floatString = _input.Substring(start, end - start);
            return float.Parse(floatString);
        }

        private char GetCurrentCharacter()
        {
            var c = _currentPos < _input.Length ? _input[_currentPos] : '\0';
            return c;            
        }

        private void ReportSyntaxError(string expected)
        {
            var character = GetCurrentCharacter() == '\0' ? "Eof" : GetCurrentCharacter().ToString();
            throw new Exception($"Expected {expected}. Got '{character}'");
        }
    }
}