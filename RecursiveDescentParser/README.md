# Recursive descent parser for mathematical expression language

This project implements a recursive descent LL(1) parser for a mathematical
expression language. Using precedence climbing, it supports correct handling of
associativity and precedence for the unary - operator, for the binary +, -, *,
/, ^ operators. It supports both integer and float type of operands.

The [BNF](https://en.wikipedia.org/wiki/Backus%E2%80%93Naur_form) grammar below
isn't the final one. It's presented as a textbook example. We evolve it into a
more readable and capable version:

    Expression = Term | Expression "+" Term | Expression "-" Term
    Term = Factor | Term "*" Factor | Term "/" Factor 
    Factor = Power | Factor "^" Power
    Power = Integer | "(" Expression ")"
    Integer = Digit | Integer Digit    
    Digit = "0" | "1" | "2" | "3" | "4" | "5" | "6" | "7" | "8" | "9" 

The ```Integer``` and ```Digit``` rules are handled by the lexer and included
for completeness. The lexer generally handles rules which can be defined by a
regular language, i.e., ones for which a regular expression can be defined.

It might help to brush up on terminology from
[algebra](https://study.com/academy/lesson/parts-of-an-expression-terms-factors-coefficients.html):
terms are parts of an expression connected with addition and subtraction and
factors are parts of an expression connected with multiplication or division.

Studying the above grammar, it appears as if the rules are off-by-one. The
```Expression``` rule describes addition and subtraction, the ```Term``` rule
multiplication and division, and so on. That's how a expression grammar tends to
show up in the literature. It's written on this form for brevity. If we expand
the first rule into two, we end up with the grammar below. Since we introduced
an extra level, we need a new name for the last rule, here named ```Primary```:

    Expression = Term
    Term = Factor | Term "+" Factor | Term "-" Factor
    Factor = Power | Factor "*" Power | Factor "/" Power
    Power = Primary | Factor "^" Primary
    Primary = Integer | "(" Expression ")"
 
In general, a grammar ```A = B```, ```B = C | D``` can be rewritten as ```A = B
| C | D```.

Rule names doesn't matter. Their inter-relationships do. Names might as well
read Expression0 to ExpressionN, denoting the level of precedense for operators
in the rule. In a recursive descent parser the only way to define operator
precedence is by recursive sub-rules. These define what part of the input will
be grouped together.

Because the rule self-reference appears on the left side, as in ```Expression
"+" Term```, it's left recursive. It expresses left associativity by design,
e.g., ```a - b - c = (a - b) - c```. Right associative operators such as unary -
or ^ would require making rule right recursive. Sometimes grammars communicate
left and right operator associativity, sometimes they keep all rules left
associative for readability. Regardless, the ```Factor``` rule above must be
implemented as if it reads ```Factor = Power | Factor "^" Factor```.

Unfortunately, recursive descent parsers cannot handle left recursion. Suppose
it were to parse ```Expression = Expression "+" Term```. In order to parse
Expression it would have to recurse into itself, leading to an infinite loop.

That's where
[EBNF](https://en.wikipedia.org/wiki/Extended_Backus%E2%80%93Naur_form) comes
in. Instead of left recursion ```{ x }``` syntax is introduced to express zero
or more repetitions. Such a representation is well suitable for coding since the
repetition can be expressed naturally with a loop.

**Side note**: A recursive descent parser has no issue with right recursive
rules. So instead of expressing left recursive rules as loops, an alternative
might be to change the original grammar to become right recursive and retain the
recursive calls. We might then apply a transformation on the constructed
[Abstract Syntax Tree](https://en.wikipedia.org/wiki/Abstract_syntax_tree) (AST)
node for left associative operations. But that's unnecessarily complex over
rewriting the grammar to the one below:

    Expression = Term
    Term = Factor | { "+" Factor } | { "-" Factor }
    Factor = Power | { "*" Power } | { "/" Power }
    Power = Primary | { "^" Primary }
    Primary = Integer | "(" Expression ")"

Favoring readability over compactness, and extending the grammar with unary
- and float support, we end up with this final grammar which is what the parser
implements. Naming a rule ```Addition``` or ```Multiplication``` is short for
addition or multiplication-like precedence.

    // Parser rules
    Expression = Addition
    Addition = Multiplication | { "+" Multiplication } | { "-" Multiplication }
    Multiplication = Power | { "*" Power } | { "/" Power }    
    Power = Unary | { "^" Power }
    Unary = '-' Unary | Primary
    Primary = Integer | Float | "(" Expression ")"

    // Lexer rules
    Float = Integer "." Integer
    Integer = Digit | { Digit }
    Digit = "0" | "1" | "2" | "3" | "4" | "5" | "6" | "7" | "8" | "9"    

In the parser, each rule is matched by a corresponding Parse method. Hitting a
breakpoint inside one of those methods, the call stack shows the path from the
```Expression``` rule to the current one being parsed. This is both useful in
debugging the parser and illustrates the close relationship between a grammar
and a recursive descent parser.

**Side note**: using additional EBNF syntax and the Kleene star, the grammar can
be further simplified.

Instead of evaluating the expression as we parse it, we could've returned
Abstract Syntax Tree nodes from each parse method. We know that the evaluation
order is correct with respect to associativity and precedence, so the Abstract
Syntax Tree most be as well. Writing an evaluator for the AST, the interpreter
would no longer be one-pass but two-pass.

## Build

    $ dotnet build
    $ dotnet test RecursiveDescentParser.Tests 
    $ dotnet run -p RecursiveDescentParser.Cli

## References

- Niklaus Wirth's [Compiler construction](https://www.inf.ethz.ch/personal/wirth/CompilerConstruction/CompilerConstruction1.pdf), Chapter 2 through to Section 4.1 (12 pages). With examples, these pages cover almost everything required to write a recursive descent parser.

- Per Vognsen's [Programming an x64 compiler from scratch - part 2](https://www.youtube.com/watch?v=Mx29YQ4zAuM), offsets 2h30m to 3h28m. Implements a simple expression parser in C. Also [Bitwise, Day 2: C Programming & Parsing](https://www.youtube.com/watch?v=0woxSWjWsb8) and [Bitwise, Day 3: More Programming & Parsing](https://www.youtube.com/watch?v=L4P98pGhpnE) are worth a look, though with some overlap.

- Bob Nystrom's [Crafting Interpreter](http://craftinginterpreters.com/contents.html), Chapter 6. Details how to modify an expression grammer to encode precedence levels.

- Eli Bendersky's [Some problems of recursive descent
  parsers](https://eli.thegreenplace.net/2009/03/14/some-problems-of-recursive-descent-parsers).
  Details how to transform a right recursive grammar into repetitions and how to
  handle left and right associative operators.