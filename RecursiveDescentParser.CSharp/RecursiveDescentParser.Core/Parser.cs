using System;
using System.Diagnostics;

namespace RecursiveDescentParser.Core;

public class Tracer
{
    private int _indentation;

    public void Enter(string method, Token token)
    {
        Console.WriteLine($"{new string(' ', _indentation)}Enter: {method}, Literal: {token.Lexeme}");
        Console.Out.Flush();
        _indentation += 4;
    }

    public void Exit(IExpression value) 
    {
        _indentation -= 4;
        Console.WriteLine($"{new string(' ', _indentation)}Exit: Type: {value.GetType().Name}");
        Console.Out.Flush();
    }
}

public class Parser
{
    private readonly Lexer _lexer;
    private readonly Tracer _tracer;
    private Token _currentToken;

    public Parser(Lexer lexer, Tracer tracer)
    {
        _lexer = lexer;
        _tracer = tracer;

        // Initialize parser state.
        NextToken();
    }

    public IExpression Parse()
    {
        _tracer.Enter(nameof(Parse), _currentToken);
        var value = ParseExpression();
        ExpectToken(TokenKind.Eof);
        _tracer.Exit(value);
        return value;
    }

    // Expression = Addition
    private IExpression ParseExpression()
    {
        _tracer.Enter(nameof(ParseExpression), _currentToken);
        var value = ParseAddition();
        _tracer.Exit(value);
        return value;
    }

    // Addition = Multiplication | { "+" Multiplication } | { "-" Multiplication }
    private IExpression ParseAddition()
    {
        _tracer.Enter(nameof(ParseAddition), _currentToken);

        // We handle left recursive rules by turning them into iterations. The
        // first call to ParseMultiplication() evaluates the left-hand side of
        // the addition and if the token following it is a Plus, we evaluate the
        // right-hand side.
        //
        // This parser deals correctly with precedence because ParseAddition
        // consumes as much of the input as it can. What's left, it'll pass to
        // ParseMultiplication() which repeats the process.
        //
        // This parser deals correctly with associativity, as in a - b - c,
        // because ParseAddition first consumes a. Then the while loop will
        // consume b, keeping track of the accumulated value. From accumulated
        // value, it'll subtract c, effectively computing (a - b) - c.
        //
        // Alternative: we could extend the hardcoded operators with a
        // table-driven approach for an extensible grammar. Then instead of
        // multiple parse methods deferring to each other, we'd have one
        // ParseExpression method recursing into itself, passing in the current
        // precedence level. Based on precedence level, we could look up the
        // operators with that precedence in the table and match on in the loop.
        // Then we'd have implemented an explicit precedence climbing parser.
        //
        // By the time ParseMultiplication returns, we've already consumed any
        // higher precedence stuff in the token stream such as multiplication,
        // power, or parenthesis.
        var left = ParseMultiplication();

        // Alternative: suppose the lexer had many token kinds for which to
        // check. Then adding each kind to the while condition would be
        // cumbersome and inefficient. Instead convert the or-check into a
        // range-check by adding to the TokenKind enum FirstPlus and LastPlus
        // like
        //
        // FirstPlus,
        // Add = FirstPlus,
        // Minus,
        // LastPlus = Minus,
        //
        // The while condition would then become: 
        //
        // TokenKind.FirstPlus <= _currentToken.Kind && _currentToken.Kind
        // <= TokenKind.LastPlus
        //
        // We may create a helper method such is IsPlusOperator() encapsulating
        // the expression. This approach would only work if all token kinds are
        // explicitly declared.
        while (IsToken(TokenKind.Plus) || IsToken(TokenKind.Minus))
        {
            var op = _currentToken.Kind;
            NextToken();
            if (op is TokenKind.Plus or TokenKind.Minus)
            {
                var right = ParseMultiplication();
                left = new InfixExpression(_currentToken, left, op, right);
            }
        }

        _tracer.Exit(left);
        return left;
    }

    // Multiplication = Power | { "*" Power } | { "/" Power }
    private IExpression ParseMultiplication()
    {
        _tracer.Enter(nameof(ParseMultiplication), _currentToken);

        var left = ParsePower();
        while (IsToken(TokenKind.Multiplication) || IsToken(TokenKind.Division))
        {
            var op = _currentToken.Kind;
            NextToken();
            if (op is TokenKind.Multiplication or TokenKind.Division)
            {
                var right = ParsePower();
                left = new InfixExpression(_currentToken, left, op, right);
            }
        }

        _tracer.Exit(left);
        return left;
    }

    // Power = Unary | { "^" Power }
    private IExpression ParsePower()
    {
        _tracer.Enter(nameof(ParsePower), _currentToken);
        var left = ParseUnary();

        // Compared to ParseAddition() and ParseMultiplication(), as there's
        // only one operator at this level, we don't need to save token kind
        // before advanced to next token.
        //
        // Because ^ is right associative, we can replace the while loop with an
        // if. ParsePower is self-recursive so the loop is implicit.
        if (IsToken(TokenKind.Power))
        {
            NextToken();

            // ^ is a right associative operator, but by calling
            //
            // var power = ParseUnary()
            //
            // below as with the other rules, ^ would become left associative.
            // For a^b^c, the evaluation would be (a^b)^c when it should be
            // a^(b^c). Instead, below we call ParsePower() to reflect the
            // grammar rule, making the method self-recursive Previous left
            // recursive rules weren't self-recursive but at this stage called
            // into the next rule.
            //
            // Parsing a^b^c, on first call, ParseUnary() above parses a. Then ^
            // is found and the call to ParsePower below causes the to the
            // method to self-recurse. On the second time around, ParseUnary()
            // parses b, once again identifies ^ and self-recurses. The third
            // time around, ParseUnary() parses c, but since it isn't followed
            // by ^ the if part is skipped and c is returned to the caller.
            // Execution picks up at the callsite of second recursion where the
            // value of b^c is calculated and returned. Then execution picks up
            // at the callsite of the first recursion where the value of
            // a^(b^c) is calculated and returned.
            //
            // And thus through self-recursion, we've made ParsePower evaluate
            // the part of the expression in a right associative manner.
            left = new InfixExpression(_currentToken, left, TokenKind.Power, ParsePower());
        }

        _tracer.Exit(left);
        return left;
    }        

    // Unary = '-' Unary | Primary
    private IExpression ParseUnary()
    {
        _tracer.Enter(nameof(ParseUnary), _currentToken);

        if (MatchToken(TokenKind.Minus))
        {
            // Like with ParsePower(), because ParseUnary() implements a right
            // recursive grammar rule, it handles not just -2 but --2, ---2 and
            // so on correctly. The latter is parsed as -(-(-2)) If we only
            // wanted to allow a single sign, we could change ParseUnary() below
            // to ParsePrimary().
            var value = new PrefixExpression(_currentToken, TokenKind.Minus, ParseUnary());
            _tracer.Exit(value);
            return value;
        } 
        else
        {
            var value = ParsePrimary();
            _tracer.Exit(value);
            return value;
        } 
    }

    // Primary = Integer | Float | "(" Expression ")"
    private IExpression ParsePrimary()
    {
        _tracer.Enter(nameof(ParsePrimary), _currentToken);

        if (IsToken(TokenKind.Integer))
        {
            var literal = new IntegerLiteral(_currentToken, (long)_currentToken.Literal!);
            NextToken();
            _tracer.Exit(literal);
            return literal;
        }
        if (IsToken(TokenKind.Float))
        {
            // We semantically call it a float but use the C#'s double type to
            // represent it. The higher precision of double over float leads to
            // fewer rounding error. With float, an input of "3.14" would become
            // 3.14000010490417 when printed with ToString().
            var literal = new FloatLiteral(_currentToken, (double)_currentToken.Literal!);
            NextToken();
            _tracer.Exit(literal);
            return literal;               
        }
        if (MatchToken(TokenKind.LParen))
        {
            var value = ParseExpression();
            ExpectToken(TokenKind.RParen);
            _tracer.Exit(value);
            return value;
        }

        // If token hasn't been consumed earlier in the call chain we end up
        // here. Syntax errors reported deal with known tokens in unexpected
        // places, such as "2+(" as well as unknown tokens, such as%, which the
        // lexer returns with a token kind of Illegal.
        ReportSyntaxError([TokenKind.Integer, TokenKind.Float, TokenKind.LParen]);
        throw new UnreachableException();
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
        return false;
    }

    private void ExpectToken(TokenKind kind)
    {
        if (IsToken(kind))
            NextToken();
        else
            ReportSyntaxError(kind);
    }

    private void ReportSyntaxError(TokenKind expected) => ReportSyntaxError([expected]);

    private void ReportSyntaxError(TokenKind[] expected)
    {           
        var kinds = $"'{string.Join("', '", expected)}'";            
        throw new Exception($"Expected {kinds}. Got '{_currentToken.Kind}'");
    }
}