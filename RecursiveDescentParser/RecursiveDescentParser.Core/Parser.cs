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
            // The basic idea to handle left recursion is to turn the recursion
            // into iteration. The first call to ParseTerm() evaluates the
            // left-hand side of the addition, so if the following token is a
            // Plus, we want to evaluate the right-hand side of the addition. We
            // can't tell from this code if it's left associative because Plus
            // is commutative. Minus isn't, however and it yields the correct
            // result, e.g., 20 - 7 - 2 = (20 - 7) - 2 = 11.
            //
            // The same technique can be used with right associative operators,
            // too. Take for instance exponentiation where 2^3^5 = 2^(3^5) 
            //
            // The reason that this parser deals correctly with precedence is
            // that ParseTerm will recursively consume as much of the input as
            // it can, and in what's left over it look for pluses and minuses in
            // that. Suppose the input is "a * b + c", then ParseTerm() will
            // consume a * b and return what it's accumulated. That's why the
            // precedence hierarchy matches the call tree.
            //
            // The reason that this part deals correctly with associativity in
            // "a - b - c" is that ParseTerm will form consume a. Then the while
            // loop will consume first consume b, keeping track of an
            // accumulator. From that accummulated value it'll subtract c,
            // effectively computing (a - b) - c.
            //
            // We could construct a table-driven approach for an extensible
            // grammar, without creating additional methods. For instance by
            // passing a precedence level to ParseExpression. Based on the
            // presedence level, we could look up the operators with that
            // precedence in the table to match on in the loop.
            var value = ParseMultiplication();
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

        // Power = Unary | { "^" Unary }
        private double ParsePower()
        {
            var value = ParseUnary();

            // Compared to ParseExpression() and ParseTerm(), we don't need to
            // save the op = _token.Type before ReadToken() as there can only be
            // one operator at this level. Also, because of the change below to
            // make Power right associative, we can replace the while loop with
            // an if. Because ParseFactor is self-recursive the loop is
            // implicit.
            if (IsToken(TokenKind.Power))
            {
                NextToken();

                // Power is a right associative operator, but by calling
                //
                // var power = ParsePower()
                //
                // as with the other productions, Power would also be left
                // associative. For a^b^c, the evaluation would be (a^b)^c
                // rather than the correct a^(b^c).
                //
                // If instead we call ParseFactor(), making the method recurse
                // into itself. With the example before, the first time
                // ParsePower() is called value = a. Then it'll see
                // TokenType.Power which will cause the while loop to evaluate
                // the remaining b^c before returning its value to previous
                // invocation of ParseFactor. And thus, right associativity is
                // achieved.
                //
                // Strictly speaking the grammar rule should be changed to
                // reflect right associativity: Power = Unary | { "^" Power }.
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
                // Because of the call to ParseUnary(), we parse not just -2 but
                // --2, ---2 and so forth correctly. If we only wanted to allow
                // a sigle sign, we'd change ParseUnary() to ParsePrimary().
                // Note also that the unary operator is right associative, i.e.,
                // --42 = -(-42). Unlike with ParsePower() the order in which
                // the change of sign is applied doesn't affect the outcome.
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
                // Even though we semantically call it a float, we use the
                // double type to represent it. The higher precision of double
                // over float leads to fewer rounding error. With float, an
                // input of "3.14" would become 3.14000010490417.
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

            // If token isn't matched ealier in the call chain we end up here.
            // The syntax errors reported deal with known tokens in an
            // unexpected place, such as "2+(". Unknown tokens, such % are
            // reported by the lexer.
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