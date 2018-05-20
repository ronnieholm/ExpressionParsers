# Recursive descent parser

This parser implements the following BNF grammar using an LL(1) recursive
descent parser:

    Expression := Term | Expression "+" Term | Expression "-" Term
    Term := Factor | Term "*" Factor | Term "/" Factor 
    Factor = Power | Factor "^" Power
    Power := Integer | "(" Expression ")"
    Integer := Digit | Integer Digit
    Digit := "0" | "1" | "2" | "3" | "4" | "5" | "6" | "7" | "8" | "9" 

Or expressed in more comfortable EBNF where instead of left recursion the syntax
of { x } is used to express zero or more repetition. Such a representation is
well suitable for coding, because the repetition can be expressed naturally with
a loop. A recursive descent parser cannot work with left recursive because then
to parse an expression it would first need to parse expression, ad infinitum.
Instead left recursive production must be rewritten to repetition. The
alternative would be to make the grammar above right recursive, which a
recursive descent parser can handle, and the apply a transformation the
constructed AST node for the operations which are left associative. But that's
unnecessary complexity over rewriting the grammar to the one below:

    Expression := Term | { "+" Term } | { "-" Term }
    Term := Factor | { "*" Factor } | { "/" Factor }
    Factor := Power | { "^" Power }
    Power := Integer | "(" Expression ")" 
    Integer := Digit | { Digit }
    Digit := "0" | "1" | "2" | "3" | "4" | "5" | "6" | "7" | "8" | "9"

The Integer and Digit productions are handled by the lexer.

It might help to brush up on terminology from
[algebra](https://sciencing.com/difference-between-term-factor-algebra-8583517.html):
A term is a constant or expression that appear in addition or subtraction and a
factor is a constant or expression that appear in multiplication or division. A
more descriptive grammar might be this one:

    Expression := Addition
    Addition := Multiplication | { "+" Multiplication } | { "-" Multiplication }
    Multiplication := Power | { "*" Power } | { "/" Power }
    Power = Primary | { "^" Primary }
    ---
    Power = Unary | { "^" Unary }
    Unary := '-' Unary | Primary
    ---
    Primary := Integer | "(" Expression ")" 
    Integer := Digit | { Digit }
    Digit := "0" | "1" | "2" | "3" | "4" | "5" | "6" | "7" | "8" | "9"

--
Or we could simple call the rule expr0, ... exprN in order of precedence
--

However, the original grammar above is commonly presented in the literature.
Don't pay too much attention to its production names. It's the rule hierarchy
that matters.

Because the self-reference to a production appear on the left side, as in
Expression "+" Term, the grammar is left recursive. It expresses left
associativity by design, e.g., a - b - c = (a - b) - c. Including a right
associative operator such as power requires additional work in the parser but
not in grammar.

It's the hierarchy of Expression, Term, Factor, and Power which define operator
precedence. In recursive descent parsers the only way to define this precedence
is by using recursive sub-rules to define what will be grouped together.

Instead of evaluating the expression as we parse it, we could've returned
abstract syntax tree nodes from each parse method. Because we know the
evaluation is correct with respect to associativity and precedence, so would the
abstract syntax tree be.

- [Compiler construction](https://www.inf.ethz.ch/personal/wirth/CompilerConstruction/CompilerConstruction1.pdf)
by Niklaus Wirth, Chapter 2 through to 4.1 (12 pages). These pages describes,
with examples, almost everything required to construct a recursive descent
parser.

- For an example of how to implement a similar expression parser in C, refer to
[Programming an x64 compiler from scratch - part 2](https://www.youtube.com/watch?v=Mx29YQ4zAuM) starting at offset 2h30m and ending at offset 3h28m.
Transforming the C implementation with its global variables and use of C idioms into C# isn't a one-to-one converstion.

- [Crafting Interpreter](http://craftinginterpreters.com/contents.html by Bob Nystrom), Chapter 6. Explains how to modify a grammer to encode precedence levels. Explains the build-up of an expression grammar slightly more advanced than ours, and with more intuitively named productions.

- [Some problems of recursive descent parsers](https://eli.thegreenplace.net/2009/03/14/some-problems-of-recursive-descent-parsers) by Eli Bendersky. Explains how to transform a right recursive grammar into repetitions and how to handle both left and right associative operators.