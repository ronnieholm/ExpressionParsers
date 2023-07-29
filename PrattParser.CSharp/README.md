# Recursive Descent Parser for mathematical expressions

## Build and run

    $ dotnet build
    $ dotnet test PrattParser.Tests 
    $ dotnet run --project PrattParser.Cli

## Algorithm

The Pratt parser is best understood through a couple of examples of running
through its algorithm:

    0: expr(rbp = 0)
    1:  let left = nud(next())
    2:  while (bp(peek()) > rbp)
    3:    left = led(left, next())
    4:  return left

with the following meanings:

  - rbp is right binding power
  - bp is a routine for looking up the binding power of operator token. 10 for
    +, 20 for *.
  - peek is a routine to peek at the next token without consuming
  - nud is short for null denotation
    - nud(operator) = Node(...)
    - nud('-') = Negatative(operand = ...)
  - led is left denotation
    - led(left, '-') = Subtraction(left = left, right = ...)
    - led(left, operator) = Tree(left, operator, right = expr(bp(operator) - 1))
  - Tree is a function that constructs a non-leaf node from its inputs

For the algorithm to work, the expected type of operators for nud and led, as
well as lookup table used by the bp function must be defined.

In today's terminology nud would be called a prefix parser and led would be
called an infix parser.

## Example 1 (1 * 2 + 3)

Let's start out with an example of two left associative operators, one with
higher precedence than the other. x marks where the current token is pointing:

    > 1 * 2 + 3
     x

Starting out, rbp = 0 and we're calling on nud to consume the token on its right
and return an abstract syntax tree node. We have nud(integer(1)) with integer(1)
being the token returned by the lexer as the next token and IntegerLiteral(1)
expression node returned by nud.

In line 2 because bp(Multiplication) > 0, i.e., 20 > 0, we enter the loop:

    led(IntegerLiteral(1), Multiplication) = Tree(IntegerLiteral(1), Multiplication, right = expr(20))

So we determined the left node and the root of the tree node, and must
recursively call expr(20) for the right node:

    Multiplication
      IntegerLiteral(1)                     
      expr(20)

Upon entry in the new expr instantiation we have

    > 1 * 2 + 3
        x

and so on line 1 we end up with nud(integer(2)) and on line 2, because
bp(Addition) > 20, i.e., 10 > 20 is false, we end up on line 4 returning
IntegerLiteral(2) to the caller. The result in the following abstract syntax
tree:

    Multiplication
      IntegerLiteral(1)
      IntegerLiteral(2)

and lexer position is here:

    > 1 * 2 + 3
          x

Back in the earlier instantiation, because on line 2 bp(Addition) > rbp, i.e.,
10
> 0), we take a second round through the while loop, resulting in  

    led(left, Addition) = Tree(left, Addition, right = expr(10))

Once more we recurse into expr(10) for the right node, leaving us with:

    Addition
      Multiplication
        IntegerLiteral(1)
        IntegerLiteral(2)
      expr(10)

Inside expr(10) we come to line 1 resulting in nud(integer 3). On line 2, we
bp(eof) > 10, i.e., -100 > 10 which is false. IntegerLiteral(3) is returned to
called, ending up with:

    Addition
      Multiplication
        IntegerLiteral(1)
        IntegerLiteral(2)
      IntegerLiteral(3)

and lexer position here:

    > 1 * 2 + 3
              x

Back in previous expr(0) we once again test the condition bp(peek()) > rbp, i.e,
-100 > 0 which is now false, causing return of left, the entire abstract syntax
tree, to the caller of expr(0).

## Example 2 (1 ^ 2 ^ 3)

This example shows how to parse right associative operators. Right associativity
is the reason for adding "- 1" in Tree function definition. The choice of one is
arbitrary but must be smaller than the room between the operators.

So once again with the algorithm we start by calling expr(rbp = 0) and in line 1
we end up with nud(integer(1)) return IntegerLiteral(1). Next, because bp(Power)
> rbp, i.e., 30 > 0, we end the loop:

    led(left, Power) = Tree(IntegerLiteral(1), Power, right = expr(29))

and finish with this abstract syntax tree:

    Power
      IntegerLiteral(1)
      expr(29)

We then recurse into expr(29), ending up with nud(2), resulting in
IntegerLiteral(2). One line 2 we have bp(Power) > rbp, i.e., 30 > 29 which is
false. And here comes the reason for subtracting one (only for right associative
operators or all operators become right associative). We end up returning

    Power
      IntegerLiteral(1)
      Power
        IntegerLiteral(2)
        IntegerLiteral(3)

making power right associatiative because it appears to have higher precedence
when applied to itself.

## References

- [Jon Blow: Discussion: Making Programming Language Parsers, etc (Q&A is in
  separate video)](https://www.youtube.com/watch?v=MnctEW1oL-E). Around 1h08m48s
  explains Pratt parsing as an alternative to the AST rewriting done in JAI. It
  may not actually be Pratt parsing, but an [operator precedence parser](https://eli.thegreenplace.net/2012/08/02/parsing-expressions-by-precedence-climbing).
  Also goes into another method that does AST rewriting before returning each
  node up the stack.
- http://journal.stuffwithstuff.com/2011/03/19pratt-parsers-expression-parsing-made-easy/
- https://dev.to/jrop/pratt-parsing
- https://eli.thegreenplace.net/2010/01/02/top-down-operator-precedence-parsing
- https://matklad.github.io/2020/04/15/from-pratt-to-dijkstra.html
- https://matklad.github.io/2020/04/13/simple-but-powerful-pratt-parsing
- https://engineering.desmos.com/articles/pratt-parser