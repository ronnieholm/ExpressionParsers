# Recursive descent parser

This parser implements the following BNF grammar using a recursive descent
parser:

    Expression := Term | Expression "+" Term | Expression "-" Term
    Term := Factor | Term "*" Factor | Term "/" Factor 
    Factor = Power | Factor "^" Power
    Power := Integer | "(" Expression ")"
    Integer := Digit | Integer Digit
    Digit := "0" | "1" | "2" | "3" | "4" | "5" | "6" | "7" | "8" | "9" 

Or expressed in more readable EBNF term where instead of recursion the syntax of
{ x } is used to express simple repetition. It's equal to an arbitrary long
sequence of x, including the empty sequence:

    Expression := Term | { "+" Term } | { "-" Term }
    Term := Factor | { "*" Factor } | { "/" Factor }
    Factor = Power | { "^" Power }
    Power := Integer | "(" Expression ")" 
    Integer := Digit | { Digit }
    Digit := "0" | "1" | "2" | "3" | "4" | "5" | "6" | "7" | "8" | "9"

It might helps to brush up on the difference between a term and a factor from
[algebra](https://sciencing.com/difference-between-term-factor-algebra-8583517.html).
In short:

- Term: constants, variables, and expressions that appear in addition or
  subtraction.
- Factor: constants, variables, and expressions that appear in multiplication.

Because the self-reference to a production appear on the left side (as in
Expression "+" Term) the grammar is said to be left recursive, i.e., it
correctly expresses left associativity by design, e.g., a - b - c = (a - b) - c.
Including a right associative operator such as power requires additional work in
the parser but not in grammar.

It's the hierarchy of Expression, Term, Factor, and Power which defines operator
precedence. Notice that the parenthesized expression is part of Power making
parenthesis the highest precedence. We could add additional levels of
precedence, for instance by including bitwise operators, at the appropriate
place in the hierarchy.

Instead of evaluating the expression as we parse it, we could return abstract
syntax tree nodes. Because we know the evaluation is correct so would the
abstract syntax tree.

- [Compiler construction](https://www.inf.ethz.ch/personal/wirth/CompilerConstruction/CompilerConstruction1.pdf)
by Niklaus Wirth, Chapter 2 through to 4.1 (12 pages). These pages describes,
with examples, almost everything required to construct a recursive descent
parser.

- For an example of how to implement a similar expression parser in C, refer to
[Programming an x64 compiler from scratch - part 2](https://www.youtube.com/watch?v=Mx29YQ4zAuM) starting at offset 2h30m and ending at offset 3h28m.
Transforming the C implementation with its global variables and use of C idioms into C# isn't a one-to-one converstion.

- [Crafting Interpreter](http://craftinginterpreters.com/contents.html by Bob
Nystrom), Chapter 6. Explains how to modify a grammer to encode precedence
levels.

- [Some problems of recursive descent parsers](https://eli.thegreenplace.net/2009/03/14/some-problems-of-recursive-descent-parsers) by Eli Bendersky.