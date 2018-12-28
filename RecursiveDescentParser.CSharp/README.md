# Recursive Descent Parser for mathematical expressions

## Build and run

    $ dotnet build
    $ dotnet test RecursiveDescentParser.Tests 
    $ dotnet run -p RecursiveDescentParser.Cli

# Examples

These examples show Parse methods calling into each other. The calls follow the
grammar, starting with the lowest and ending with the highest precedence
operator.

For a single integer, we see how every parsing rule is activated in order of
precedence:

```
> 1
Enter: Parse, Value: 1
    Enter: ParseExpression, Value: 1
        Enter: ParseAddition, Value: 1
            Enter: ParseMultiplication, Value: 1
                Enter: ParsePower, Value: 1
                    Enter: ParseUnary, Value: 1
                        Enter: ParsePrimary, Value: 1
                        Exit: Value: 1
                    Exit: Value: 1
                Exit: Value: 1
            Exit: Value: 1
        Exit: Value: 1
    Exit: Value: 1
Exit: Value: 1
1
```

Here's the same integer parenthesized. As before, we travel the rules until
ParsePrimary is reached. Parenthesis trumps the precedence of all other
operators and causes and recursively calls back into ParseExpression for the
parenthesized expression to be evaluated before anything else:

```
> (1)
Enter: Parse, Value: 
    Enter: ParseExpression, Value: 
        Enter: ParseAddition, Value: 
            Enter: ParseMultiplication, Value: 
                Enter: ParsePower, Value: 
                    Enter: ParseUnary, Value: 
                        Enter: ParsePrimary, Value: 
                            Enter: ParseExpression, Value: 1
                                Enter: ParseAddition, Value: 1
                                    Enter: ParseMultiplication, Value: 1
                                        Enter: ParsePower, Value: 1
                                            Enter: ParseUnary, Value: 1
                                                Enter: ParsePrimary, Value: 1
                                                Exit: Value: 1
                                            Exit: Value: 1
                                        Exit: Value: 1
                                    Exit: Value: 1
                                Exit: Value: 1
                            Exit: Value: 1
                        Exit: Value: 1
                    Exit: Value: 1
                Exit: Value: 1
            Exit: Value: 1
        Exit: Value: 1
    Exit: Value: 1
Exit: Value: 1
1
```

For addition, we see repeated calls to the rule beyond addition, namely
multiplication. That's because inside ParseAddition, for each "+ operand"
encountered, we call ParseMultiplication in a loop.

```
> 1+2
Enter: Parse, Value: 1
    Enter: ParseExpression, Value: 1
        Enter: ParseAddition, Value: 1
            Enter: ParseMultiplication, Value: 1
                Enter: ParsePower, Value: 1
                    Enter: ParseUnary, Value: 1
                        Enter: ParsePrimary, Value: 1
                        Exit: Value: 1
                    Exit: Value: 1
                Exit: Value: 1
            Exit: Value: 1
            Enter: ParseMultiplication, Value: 2
                Enter: ParsePower, Value: 2
                    Enter: ParseUnary, Value: 2
                        Enter: ParsePrimary, Value: 2
                        Exit: Value: 2
                    Exit: Value: 2
                Exit: Value: 2
            Exit: Value: 2
        Exit: Value: 3
    Exit: Value: 3
Exit: Value: 3
3
```

Addition is a left associative operator. As an example of a right associative
operator, let's see what parsing a power expression results in. Looking at the
indentation, we see a subtle difference. The initial path down to ParsePrimary
is the same, but we don't return three levels up, continuing with the next loop
iteration. Instead, ParsePower makes a self-recursive call. It's this
self-recursion that makes the rule right associative.

```
> 2^3
Enter: Parse, Value: 2
    Enter: ParseExpression, Value: 2
        Enter: ParseAddition, Value: 2
            Enter: ParseMultiplication, Value: 2
                Enter: ParsePower, Value: 2
                    Enter: ParseUnary, Value: 2
                        Enter: ParsePrimary, Value: 2
                        Exit: Value: 2
                    Exit: Value: 2
                    Enter: ParsePower, Value: 3
                        Enter: ParseUnary, Value: 3
                            Enter: ParsePrimary, Value: 3
                            Exit: Value: 3
                        Exit: Value: 3
                    Exit: Value: 3
                Exit: Value: 8
            Exit: Value: 8
        Exit: Value: 8
    Exit: Value: 8
Exit: Value: 8
8
```

For addition followed by multiplication, we see the same repeated calls to the
rule beyond addition, namely multiplication. But because multiplication has
higher precedence than addition, multiplication must be carried out first. This
happen with the second call to ParseMultiplication. Inside ParseMultiplication,
if the operand is followed by '*', it calls ParsePower in a loop:

```
> 1+2*3
Enter: Parse, Value: 1
    Enter: ParseExpression, Value: 1
        Enter: ParseAddition, Value: 1
            Enter: ParseMultiplication, Value: 1
                Enter: ParsePower, Value: 1
                    Enter: ParseUnary, Value: 1
                        Enter: ParsePrimary, Value: 1
                        Exit: Value: 1
                    Exit: Value: 1
                Exit: Value: 1
            Exit: Value: 1
            Enter: ParseMultiplication, Value: 2
                Enter: ParsePower, Value: 2
                    Enter: ParseUnary, Value: 2
                        Enter: ParsePrimary, Value: 2
                        Exit: Value: 2
                    Exit: Value: 2
                Exit: Value: 2
                Enter: ParsePower, Value: 3
                    Enter: ParseUnary, Value: 3
                        Enter: ParsePrimary, Value: 3
                        Exit: Value: 3
                    Exit: Value: 3
                Exit: Value: 3
            Exit: Value: 6
        Exit: Value: 7
    Exit: Value: 7
Exit: Value: 7
7
```