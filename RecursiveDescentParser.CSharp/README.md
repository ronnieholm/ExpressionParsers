# Recursive Descent Parser for mathematical expressions

## Build and run

    $ dotnet build
    $ dotnet test RecursiveDescentParser.Tests 
    $ dotnet run -p RecursiveDescentParser.Cli

# Examples

These examples shows the Parse methods call each other, following the layer of grammar, starting
with the lowest and ending with the highest presedence operator.

For single integer, we see how every parsing rule is activated in their order of presedence:

```
> 1
Rule: Parse, Kind: Integer, Value: 1
    Rule: ParseExpression, Kind: Integer, Value: 1
        Rule: ParseAddition, Kind: Integer, Value: 1
            Rule: ParseMultiplication, Kind: Integer, Value: 1
                Rule: ParsePower, Kind: Integer, Value: 1
                    Rule: ParseUnary, Kind: Integer, Value: 1
                        Rule: ParsePrimary, Kind: Integer, Value: 1
                        Result: 1
                    Result: 1
                Result: 1
            Result: 1
        Result: 1
    Result: 1
Result: 1
```

And the same single integer parenthesized. We first go down the rules until ParsePrimary is reached.
The rule that matched is the one with the parenthesis causing a call back to ParseExpression for
the whole way through the rules once again. This is what causes parenthesized expression to be
parsed first, setting aside any presedence rule of what surrounds the parenthesis:

```
> (1)
Rule: Parse, Kind: LParen, Value: 
    Rule: ParseExpression, Kind: LParen, Value: 
        Rule: ParseAddition, Kind: LParen, Value: 
            Rule: ParseMultiplication, Kind: LParen, Value: 
                Rule: ParsePower, Kind: LParen, Value: 
                    Rule: ParseUnary, Kind: LParen, Value: 
                        Rule: ParsePrimary, Kind: LParen, Value: 
                            Rule: ParseExpression, Kind: Integer, Value: 1
                                Rule: ParseAddition, Kind: Integer, Value: 1
                                    Rule: ParseMultiplication, Kind: Integer, Value: 1
                                        Rule: ParsePower, Kind: Integer, Value: 1
                                            Rule: ParseUnary, Kind: Integer, Value: 1
                                                Rule: ParsePrimary, Kind: Integer, Value: 1
                                                Result: 1
                                            Result: 1
                                        Result: 1
                                    Result: 1
                                Result: 1
                            Result: 1
                        Result: 1
                    Result: 1
                Result: 1
            Result: 1
        Result: 1
    Result: 1
Result: 1
```

For addition, we see repeated calls to the rule beyond addition, namely Multiplication.
That's because inside ParseAddition, for each operand, we call ParseMultiplication in a loop.

```
Rule: Parse, Kind: Integer, Value: 1
    Rule: ParseExpression, Kind: Integer, Value: 1
        Rule: ParseAddition, Kind: Integer, Value: 1
            Rule: ParseMultiplication, Kind: Integer, Value: 1
                Rule: ParsePower, Kind: Integer, Value: 1
                    Rule: ParseUnary, Kind: Integer, Value: 1
                        Rule: ParsePrimary, Kind: Integer, Value: 1
                        Result: 1
                    Result: 1
                Result: 1
            Result: 1
            Rule: ParseMultiplication, Kind: Integer, Value: 2
                Rule: ParsePower, Kind: Integer, Value: 2
                    Rule: ParseUnary, Kind: Integer, Value: 2
                        Rule: ParsePrimary, Kind: Integer, Value: 2
                        Result: 2
                    Result: 2
                Result: 2
            Result: 2
        Result: 3
    Result: 3
Result: 3
```

Addition is a left associative operator. As an example of a right associate operator
let's see what the parsing an expression involved Power results in. Just looking at
the indentation, we see a subtle difference. The initial path down to ParsePrimary
is the same, but we don't return three levels back to continue with a loop. Instead,
ParsePower makes a self-recursive call. It's this self-recursion that makes the rule
right associative.

```
Rule: Parse, Kind: Integer, Value: 2
    Rule: ParseExpression, Kind: Integer, Value: 2
        Rule: ParseAddition, Kind: Integer, Value: 2
            Rule: ParseMultiplication, Kind: Integer, Value: 2
                Rule: ParsePower, Kind: Integer, Value: 2
                    Rule: ParseUnary, Kind: Integer, Value: 2
                        Rule: ParsePrimary, Kind: Integer, Value: 2
                        Result: 2
                    Result: 2
                    Rule: ParsePower, Kind: Integer, Value: 3
                        Rule: ParseUnary, Kind: Integer, Value: 3
                            Rule: ParsePrimary, Kind: Integer, Value: 3
                            Result: 3
                        Result: 3
                    Result: 3
                Result: 8
            Result: 8
        Result: 8
    Result: 8
Result: 8
```

For addition followed by multiplication, we see the same repeated calls to the rule beyond addition,
namely Multiplication. By because multiplication has higher presedence than addition, the multiplication
must be carried out first. We see this happen with the second call to ParseMultiplication. Inside
ParseMultiplication, if the operand is followed by '*', we'll match the next use, Power, in a loop.
So this organization of rules in increasing order of presedence causes the parts of the expressions
with operators of higher predence to be evaluated first.

```
Rule: Parse, Kind: Integer, Value: 1
    Rule: ParseExpression, Kind: Integer, Value: 1
        Rule: ParseAddition, Kind: Integer, Value: 1
            Rule: ParseMultiplication, Kind: Integer, Value: 1
                Rule: ParsePower, Kind: Integer, Value: 1
                    Rule: ParseUnary, Kind: Integer, Value: 1
                        Rule: ParsePrimary, Kind: Integer, Value: 1
                        Result: 1
                    Result: 1
                Result: 1
            Result: 1
            Rule: ParseMultiplication, Kind: Integer, Value: 2
                Rule: ParsePower, Kind: Integer, Value: 2
                    Rule: ParseUnary, Kind: Integer, Value: 2
                        Rule: ParsePrimary, Kind: Integer, Value: 2
                        Result: 2
                    Result: 2
                Result: 2
                Rule: ParsePower, Kind: Integer, Value: 3
                    Rule: ParseUnary, Kind: Integer, Value: 3
                        Rule: ParsePrimary, Kind: Integer, Value: 3
                        Result: 3
                    Result: 3
                Result: 3
            Result: 6
        Result: 7
    Result: 7
Result: 7
```