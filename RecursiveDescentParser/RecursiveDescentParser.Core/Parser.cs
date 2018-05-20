using System;

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

        public int Parse()
        {
            var value = ParseExpression();
            ExpectToken(TokenKind.Eof);
            return value;
        }

        // Expression := Term | Expression "+" Term | Expression "-" Term
        // Expression := Term | { "+" Term } | { "-" Term }
        public int ParseExpression()
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
            var value = ParseTerm();
            while (IsToken(TokenKind.Plus) || IsToken(TokenKind.Minus))
            {
                var op = _currentToken.Kind;
                NextToken();
                if (op == TokenKind.Plus)
                {
                    value += ParseTerm();
                }
                else if (op == TokenKind.Minus)
                {
                    value -= ParseTerm();
                }
            }
            return value;
        }

        // Term := Factor | Term "*" Factor | Term "/" Factor 
        // Term := Factor | { "*" Factor } | { "/" Factor }
        public int ParseTerm()
        {
            var value = ParseFactor();
            while (IsToken(TokenKind.Multiplication) || IsToken(TokenKind.Division))
            {
                var op = _currentToken.Kind;
                NextToken();
                if (op == TokenKind.Multiplication)
                {
                    value *= ParseFactor();
                }
                else if (op == TokenKind.Division)
                {
                    value /= ParseFactor();
                }
            }
            return value;
        }

        // Factor = Power | Factor "^" Power
        // Factor = Power | { "^" Power }
        public int ParseFactor()
        {
            var value = ParsePower();

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
                var power = ParseFactor();
                var base_ = value;
                value = 1;
                for (var i = 0; i < power; i++)
                {
                    value *= base_;
                }
            }
            return value;
        }        

        // Power := Integer | "(" Expression ")" 
        public int ParsePower()
        {
            if (IsToken(TokenKind.Integer))
            {
                var integer = Convert.ToInt32(_currentToken.Value);
                NextToken();
                return integer;
            }
            else if (MatchToken(TokenKind.LParen))
            {
                var value = ParseExpression();
                ExpectToken(TokenKind.RParen);
                return value;
            }
            throw new Exception($"Expected 'Integer' or 'LParen'. Got '{_currentToken.Kind}'");
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
                throw new Exception($"Expected token '{kind}'. Got '{_currentToken.Kind}'");
            }
        }
    }
}