using System;

namespace RecursiveDescentParser.Core
{
    public enum TokenKind
    {
        // Alternative: enumeration could start at 128, encoding any single
        // charactor ASCII token as itself. That way, we need not explicitly
        // define tokens for the single character tokens. Then in the lexer's
        // switch statement, we could match these tokens like we do with
        // numbers, but with a simpler token return.
        //
        // This approach doesn't work as well with C# because type conversions
        // between char and integer aren't implicit. Whenever we need to
        // construct a new Token we have to do "(TokenKind)_ch" and whenever in
        // the parser we need to compare token type we need to do "_token.Kind
        // == (TokenKind)'('". Printing error messages from within the parser we
        // would have to check if the kind is < 128 and then convert it to a
        // char or have a PrintToken method do it. For kinds < 128, the error
        // message would become "Expected token: '('". With expclit tokens as
        // below it's "Expected token: LParen".
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

    // A common approach to creating a lexer is to define a Token class which
    // holds a TokenType enum and the string matched by the enum and possibly
    // offsets into the input for error reporting. That way related information
    // stay together. But because an integer or float matched isn't a string
    // (and C# doesn't support union types), it would need to be converted to a
    // string. Then when needed, the parser would have to convert the string
    // back to its actual type. These two conversions, while minor in terms of
    // performance, are redundant.
    //
    // A coulple of alternatives exist to metigate those, but unless performance
    // is truly critical, they make the code harder to read and are best
    // avoided:
    //
    // 1. Return the TokenType (possibly renamed to Token) from ReadToken()
    //    only. Calling code is then responsible for inspecting the TokenType
    //    and getting the value from a property on the lexer. It would expose
    //    values for string, integer, float, and so on. This way a token of any
    //    type can be returned and accessed without conversion. On the downside,
    //    it possible for the client to read the wrong property.
    //
    // 2. Define a generic Token<T> class where T is the of the literal, such as
    //    string, int, float, ... But then what to do for tokens with no
    //    associated type? Maybe create it as type string an possibly leave the
    //    value empty.
    //
    // 3. Define specializations of Token. But since ReadToken() returns Token,
    //    clients would be required to switch on the type to access the value.
    //
    // The only token type our lexer supports where the type doesn't uniquely
    // describe the value is the integer. In this simple lexer we could solve
    // the problem described above by turning Value into an integer.
    public class Token
    {
        // TODO: Store Start and End as well. In principle, since we have Start
        // end End we can infer value and don't need to pass it. We do pass it
        // anyway since some computation might have been involved like with the
        // integer. In some cases the value may be different than the literal
        // source text. For instance, a float in the source may be 3,14 whereas
        // its value is 3.14.
        //
        // TODO: Extend SyntaxError in lexer and parser with visual indicator of
        // error in source text.
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
            // to consume leading whitespace, but instead we decided to handle
            // whitespace in the switch statement using a goto statement. We
            // wanted a uniform treatment of characters. Consuming whitespace
            // like above biases the lexer toward whitespace. For each character
            // the while expression is evaluated which depending on the language
            // being lexer may be slightly inefficient.
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
                    // One we know whether we're at a float or integer, we'll
                    // backtrack using the position bookmark and call the
                    // appropriate scanner.
                    var bookmark = _currentPos;
                    while (char.IsDigit(GetCurrentCharacter()))
                    {
                        _currentPos++;
                    }
                    if (GetCurrentCharacter() == '.')
                    {
                        _currentPos = bookmark;
                        var float_ = ScanFloat();
                        return new Token(TokenKind.Float, float_.ToString());                       
                    }
                    else
                    {
                        _currentPos = bookmark;
                        var integer = ScanInteger();
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
                    ReportSyntaxError();
                    throw new Exception("Unreachable");
            }
        }

        // Integer = Digit | Integer Digit
        private int ScanInteger()
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
        private float ScanFloat()
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
            // ScanInteger is that the mantisse could be a number prefixed by
            // zero. In the case the integer returned wouldn't be correct.
            // Instead we locate the start and end positions of the float.
            var floatString = _input.Substring(start, end - start);
            return float.Parse(floatString);
        }

        private char GetCurrentCharacter()
        {
            var c = _currentPos < _input.Length ? _input[_currentPos] : '\0';
            return c;            
        }

        private void ReportSyntaxError()
        {
            throw new Exception($"Unexpected character. Got '{GetCurrentCharacter()}'");
        }

        private void ReportSyntaxError(string expected)
        {
            var character = GetCurrentCharacter() == '\0' ? "Eof" : GetCurrentCharacter().ToString();
            throw new Exception($"Expected {expected}. Got '{character}'");
        }
    }
}