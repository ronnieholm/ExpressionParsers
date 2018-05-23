using System;
using System.Globalization;

namespace RecursiveDescentParser.Core
{
    public class Parser
    {
        Lexer _lexer;
        Token _currentToken;

        public Parser(Lexer lexer)
        {
            _lexer = lexer;

            // Initialize parser state.
            NextToken();
        }

        public double Parse()
        {
            var value = ParseExpression();
            ExpectToken(TokenKind.Eof);
            return value;
        }

        // Expression = Addition
        private double ParseExpression()
        {
            var value = ParseAddition();
            return value;
        } 

        // Addition = Multiplication | { "+" Multiplication } | { "-" Multiplication }
        private double ParseAddition()
        {
            // We handle the left recursive rule by turning it into iterations.
            // The first call to ParseTerm() evaluates the left-hand side of the
            // addition and if the token following it is a Plus, we evaluate the
            // right-hand side.
            //
            // The reason this parser deals correctly with precedence is that
            // ParseAddition will consume as much of the input as it can. What's
            // left it'll pass to ParseMultiplication() which repeats the
            // process.
            //
            // The reason this parser deals correctly with associativity, as in
            // a - b - c, is that ParseAddition will first consume a. Then the
            // while loop will consume first consume b, keeping track of
            // accumulator. From that accummulated value it'll subtract c,
            // effectively computing (a - b) - c.
            //
            // Alternative: we could extend the hardcoded operators below with a
            // table-driven approach for an extensible grammar. Then instead of
            // multiple parse methods deferring to each other, we'd have one
            // ParseExpression method recursing into itself, passing in the
            // current precedence level. Based on the presedence level, we could
            // look up the operators with that precedence in the table and match
            // on in the loop. That would be an explicit precedence climbing
            // parser.
            var value = ParseMultiplication();

            // Alternative: suppose the lexer had many token kinds for which to
            // check. Then adding each kind to the while condition would be
            // cumbersome and inefficient. Instead convert the or check into a
            // range check by adding to TokenKind FirstPlus and LastPlus with
            // actual tokens in between. The while condition would become: 
            //
            // TokenKind.FirstPlus <= _currentToken.Kind && _currentToken.Kind
            // <= TokenKind.LastPlus
            //
            // We may even create a helper method such is IsPlusOperator()
            // encapsulating the expression. This approach would only work if
            // all token kinds are explicitly declared.
            while (IsToken(TokenKind.Plus) || IsToken(TokenKind.Minus))
            {
                var op = _currentToken.Kind;
                NextToken();
                if (op == TokenKind.Plus)
                {
                    value += ParseMultiplication();
                }
                else if (op == TokenKind.Minus)
                {
                    value -= ParseMultiplication();
                }
            }
            return value;
        }

        // Multiplication = Power | { "*" Power } | { "/" Power }
        private double ParseMultiplication()
        {
            var value = ParsePower();
            while (IsToken(TokenKind.Multiplication) || IsToken(TokenKind.Division))
            {
                var op = _currentToken.Kind;
                NextToken();
                if (op == TokenKind.Multiplication)
                {
                    value *= ParsePower();
                }
                else if (op == TokenKind.Division)
                {
                    value /= ParsePower();
                }
            }
            return value;
        }

        // Power = Unary | { "^" Power }
        private double ParsePower()
        {
            var value = ParseUnary();

            // Compared to ParseAddition() and ParseMultiplication(), as there's
            // only one operator at this level, we don't need to save token kind
            // before advanced to next token. Also, because ^ is right
            // associative, we can replace the while loop with an if. ParsePower
            // is self-recursive so the loop is implicit.
            if (IsToken(TokenKind.Power))
            {
                NextToken();

                // ^ is a right associative operator, but by calling
                //
                // var power = ParseUnary()
                //
                // below as with the other rules, ^ would be left associative.
                // For a^b^c, the evaluation would be (a^b)^c rather than the
                // correct a^(b^c). Instead we call ParsePower() to reflect the
                // grammar rule, making the method recurse into itself. 
                //
                // With a^b^c, the initial ParseUnary() evaluates a. Then ^ is
                // found and the while loop evaluate b^c before returning its
                // accumulator to the calling ParsePower(). And thus, right
                // associativity is achieved.
                var power = ParsePower();
                var base_ = value;
                value = 1;
                for (var i = 0; i < power; i++)
                {
                    value *= base_;
                }
            }
            return value;
        }        

        // Unary = '-' Unary | Primary
        private double ParseUnary()
        {
            if (MatchToken(TokenKind.Minus))
            {
                // Like with ParsePower(), because ParseUnary() implements a
                // right recursive grammar rule, it handles not just -2 but --2,
                // ---2 and so on correctly. The latter is parsed as -(-(-2)) If
                // we only wanted to allow a single sign, we'd change the call
                // to ParseUnary() below to ParsePrimary().
                var value = ParseUnary();
                return -value;
            } 
            else
            {
                var value = ParsePrimary();
                return value;
            } 
        }

        // Primary = Integer | Float | "(" Expression ")"
        private double ParsePrimary()
        {
            if (IsToken(TokenKind.Integer))
            {
                var integer = int.Parse(_currentToken.Value);
                NextToken();
                return integer;
            }
            else if (IsToken(TokenKind.Float))
            {
                // We semantically call it a float but use the C# double type to
                // represent it. The higher precision of double over float leads
                // to fewer rounding error. With float, an input of "3.14" would
                // become 3.14000010490417 when printed with ToString().
                var float_ = double.Parse(_currentToken.Value, CultureInfo.InvariantCulture);
                NextToken();
                return float_;               
            }
            else if (MatchToken(TokenKind.LParen))
            {
                var value = ParseExpression();
                ExpectToken(TokenKind.RParen);
                return value;
            }

            // If token hasn't been consumed ealier in the call chain we end up
            // here. The syntax errors reported deal with known tokens in an
            // unexpected place, such as "2+(" as well as unknown tokens, such
            // %, which the lexer returns with a token kind of Illegal.
            ReportSyntaxError(new[] { TokenKind.Integer, TokenKind.Float, TokenKind.LParen });
            throw new Exception("Unreachable");
        }

        private void NextToken() => _currentToken = _lexer.NextToken();
        private bool IsToken(TokenKind kind) => _currentToken.Kind == kind;

        private bool MatchToken(TokenKind kind)
        {
            if (IsToken(kind))
            {
                NextToken();
                return true;
            }
            else
            {
                return false;
            }
        }

        private void ExpectToken(TokenKind kind)
        {
            if (IsToken(kind))
            {
                NextToken();
            }
            else
            {
                ReportSyntaxError(kind);
            }
        }

        private void ReportSyntaxError(TokenKind expected) => ReportSyntaxError(new[] {expected});

        private void ReportSyntaxError(TokenKind[] expected)
        {           
            var kinds = $"'{string.Join("', '", expected)}'";            
            throw new Exception($"Expected {kinds}. Got '{_currentToken.Kind}'");
        }
    }
}