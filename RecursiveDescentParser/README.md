# Recursive descent parser

This parser implements the following BNF grammar using an LL(1) recursive
descent parser:

    Expression = Term | Expression "+" Term | Expression "-" Term
    Term = Factor | Term "*" Factor | Term "/" Factor 
    Factor = Power | Factor "^" Power
    Power = Integer | "(" Expression ")"
    Integer = Digit | Integer Digit
    Digit = "0" | "1" | "2" | "3" | "4" | "5" | "6" | "7" | "8" | "9" 

The Integer and Digit productions are handled by the lexer and included for
completeness.

It might help to brush up on terminology from
[algebra](https://sciencing.com/difference-between-term-factor-algebra-8583517.html):
A term is a constant or expression that appear in addition or subtraction and a
factor is a constant or expression that appear in multiplication or division.

Reading the grammar above, it appears if though the rules are off-by-one. The
rule for expression describes addition and subtraction, the rule for a term
multiplication and division, and so on. For brevity, that's how a simple
expression grammar tends to show up in the literature. If we expand the first
rule into two, we end up with the following grammar which reads more natural,
yet the two grammars are identical:

    Expression = Term
    Term = Factor | Term "+" Factor | Term "-" Factor
    Factor = Power | Factor "*" Power | Factor "/" Power
    ... 

In general, a grammar A = B, B = C | D can be rewritten as A = B | C | D.

Rule names might as well read Expression0 to ExpressionN, denoting the level of
precedense for the operator in the rule. For parsing purposes, it's the rule
hierarchy that matters, not their names. In recursive descent parsers the only
way to define operator precedence is by using recursive sub-rules to define what
will be grouped together.

Because the self-reference to a production appear on the left side, as in
"Expression "+" Term" in the first grammar, the grammar is left recursive. It
expresses left associativity by design, e.g., a - b - c = (a - b) - c. Including
a right associative operator, such as power, would require changing the grammar
accordingly. Sometimes we want to communicate operator associativity in grammar
while at other times keep all rules left associative for readability. The Factor
rule would still need to implemented as if it read "Factor = Power | Factor "^"
Factor.

Unfortunately, recursive descent parsers cannot handle left recursion. Suppose
it were to parse "Expression "+" Term". In order to parse Expression it would
have to recurse into itself, going into an infinite loop.

That's where EBNF comes in. Instead of left recursion the syntax of { x } is
introduced to express zero or more repetition. Such a representation is well
suitable for coding since the repetition can be expressed naturally with a loop.

Side note: A recursive descent parser has no issue with right recursive rules.
So instead of expressing left recursive rules as loops, an alternative might be
to change the original grammar to become right recursive and retain recursive
parse calls. We might then apply a transformation on the constructed Abstract
Syntax Tree node for left associative operations. But that's unnecessarily
complex over rewriting the grammar to the one below:

    Expression = Term | { "+" Term } | { "-" Term }
    Term = Factor | { "*" Factor } | { "/" Factor }
    Factor = Power | { "^" Power }
    Power = Integer | "(" Expression ")" 

Favoring readability over compactness of grammar, and extending the grammar with
unary minus support, we end up with this final grammar which is what the parser
implements:

    Expression = Addition
    Addition = Multiplication | { "+" Multiplication } | { "-" Multiplication }
    Multiplication = Power | { "*" Power } | { "/" Power }    
    Power = Unary | { "^" Unary }
    Unary = '-' Unary | Primary
    Primary = Integer | "(" Expression ")" 

Instead of evaluating the expression as we parse it, we could've returned
Abstract Syntax Tree nodes from each parse method. Because we know the
evaluation is correct with respect to associativity and precedence, the Abstract
Syntax Tree would be as well. Writing an evaluator for the AST, the interpreter
would no longer be one-pass but two-pass.

## References

- [Compiler construction](https://www.inf.ethz.ch/personal/wirth/CompilerConstruction/CompilerConstruction1.pdf)
by Niklaus Wirth, Chapter 2 through to 4.1 (12 pages). These pages describes,
with examples, almost everything required to construct a recursive descent
parser.

- For an example of how to implement a similar expression parser in C, refer to
[Programming an x64 compiler from scratch - part 2](https://www.youtube.com/watch?v=Mx29YQ4zAuM) starting at offset 2h30m and ending at offset 3h28m.
Transforming the C implementation with its global variables and use of C idioms into C# isn't a one-to-one converstion. https://www.youtube.com/watch?v=0woxSWjWsb8 and https://www.youtube.com/watch?v=L4P98pGhpnE are worth a cursory look with some overlap to the first video.

- [Crafting Interpreter](http://craftinginterpreters.com/contents.html by Bob Nystrom), Chapter 6. Explains how to modify a grammer to encode precedence levels. Explains the build-up of an expression grammar slightly more advanced than ours, and with more intuitively named productions.

- [Some problems of recursive descent parsers](https://eli.thegreenplace.net/2009/03/14/some-problems-of-recursive-descent-parsers) by Eli Bendersky. Explains how to transform a right recursive grammar into repetitions and how to handle both left and right associative operators.