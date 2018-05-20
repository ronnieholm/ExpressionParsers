using System;

namespace RecursiveDescentParser.Core
{
    public enum TokenKind
    {
        // Alternative: enumeration could start at 128, encoding any single
        // charactor ASCII token as itself. That way, we need not explicitly
        // define tokens for the punctuation characters, for instance. This kind
        // makes sense in a languages such as C where enum values are integers
        // and the conversion between chars and integers is implicit. In the
        // lexer switch statement, we could match all cases of punctuation
        // tokens like we do with numbers.
        //
        // Howerver, this approach doesn't work well with C# because type
        // conversions between char and integer isn't implicit. Whenever we'd
        // need to construct a new Token we'd have to do "(TokenType)_ch" and
        // whenever in the parser we'd want to compare the token then we'd need
        // to do "_token.Type == (TokenType)'('". Printing error messages from
        // within the parser, such as from the ExpectToken method, would also
        // we'd have to check if the type is < 128 and explicitly convert the
        // token type to a char (or call a PrintToken method). For token types <
        // 128, the error message would become "Expected token: '('" rather than
        // Expected token: LParen.
        Eof,
        Integer,
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
    // stay together. But because an integer or float matched isn't a string, it
    // would need to be converted to a string. Then when needed, the parser
    // would have to convert the string back to its actual type. These two
    // conversions, while minor in terms of performance, are redundant.
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
        char _currentCharacter;
        int _nextPos;

        public Lexer(string input)
        {
            _input = input;

            // Initialize lexer state.
            NextCharacter();
        }

        public Token NextToken()
        {
            // Alternative: we couldn've started of with 
            //
            // while (_ch == ' ' || _ch == '\n' || _ch == '\r' || 
            //        _ch == '\t' || _ch == 'v') 
            // {
            //     ReadCharacter();    
            // }
            //
            // which would consume leading whitespace, but instead we decided to
            // handle whitespace in the switch statement using a goto statement.
            // We wanted a uniform treatment of characters. Consuming whitespace
            // like above biases the lexer toward whitespace. For each character 
            // the while expression is evaluated which depending on the language
            // being lexer may be slightly inefficient.
        retry:
            switch (_currentCharacter)
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
                    var integer = _currentCharacter - '0';
                    NextCharacter();
                    while (char.IsDigit(_currentCharacter))
                    {
                        integer *= 10;
                        integer += _currentCharacter - '0';
                        NextCharacter();
                    }
                    return new Token(TokenKind.Integer, integer.ToString());
                case '+':
                    NextCharacter();
                    return new Token(TokenKind.Plus);
                case '-':
                    NextCharacter();
                    return new Token(TokenKind.Minus);                
                case '*':
                    NextCharacter();
                    return new Token(TokenKind.Multiplication);                
                case '/':
                    NextCharacter();
                    return new Token(TokenKind.Division);            
                case '^':
                    NextCharacter();
                    return new Token(TokenKind.Power);            
                case '(':
                    NextCharacter();
                    return new Token(TokenKind.LParen);                
                case ')':
                    NextCharacter();
                    return new Token(TokenKind.RParen);
                case ' ':
                case '\n':
                case '\r':
                case '\t':
                case '\v':
                    NextCharacter();
                    goto retry;
                default:
                    throw new Exception($"Unknown character: '{_currentCharacter}'");
            }
        }

        private void NextCharacter()
        {
            _currentCharacter = _nextPos < _input.Length ? _input[_nextPos] : '\0';
            _nextPos++;
        }
    }
}
