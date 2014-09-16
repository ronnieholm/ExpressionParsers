#if INTERACTIVE
#r "../packages/Unquote.2.2.2/lib/net40/Unquote.dll"
#r "System.dll"
#load "Lexer.fs"
#else
module Parser
#endif

(*
    Shunting Yard algorithm
    
      while tokens to be read
        read token
        if token is operand then push onto operand stack
        if token is unary prefix operator then push onto operator stack
        if token is binary operator, o1, then
          while operator token, o2, at top of operator stack, and
              either o1 is left-associative and its precedence is <= to that of o2
              or o1 is right-associative and its precedence < that of o2
            reduce expression
          push o1 onto the operator stack
        if token is left paren then push it onto operator stack
        if token is right paren
          until token at top of operator stack is left paren
            reduce expression
          if operator stack runs out without finding left paren then mismatched parens
          pop left paren from stack
      when no more tokens to readand
        while still tokens on operator stack
          if operator token on top of stack is paren then mismatched parentheses
          reduce expression
        pop result of operand stack

      Reduce expession
        pop operator off operator stack
        pop operands off operand stack
        process expression
        push result onto operand stack
*)

open System
open System.Collections.Generic
open Swensen.Unquote
open Lexer

// test <@ tokenize ("-(1+2*3/4^5)" |> toArray) = 
// [UnaryMinOp; LParen; Integer 1; BinPlusOp; Integer 2; BinMulOp; 
//  Integer 3; BinDivOp; Integer 4; BinExpOp; Integer 5; RParen] @>

type Associativity =
    | Left
    | Right

let precedense =
    [(Token.BinExpOp, 4)
     (Token.UnaryMinOp, 3)
     (Token.BinMulOp, 2)
     (Token.BinDivOp, 2)
     (Token.BinPlusOp, 1)
     (Token.BinMinOp, 1)] |> Map.ofList

let associativity =
    [(Token.BinExpOp, Right)
     (Token.UnaryMinOp, Left)
     (Token.BinMulOp, Left)
     (Token.BinDivOp, Left)
     (Token.BinPlusOp, Left)
     (Token.BinMinOp, Left)] |> Map.ofList

let operands = new Stack<Token>()
let operators = new Stack<Token>()

// todo: eventually pass into parse 
let pushOperand token =
    operands.Push(token)

let pushOperator token =
    operators.Push(token)

let isOperand token =
    match token with
    | Integer _ -> true
    | _ -> false

let isUnaryOperator token =
    match token with
    | UnaryMinOp | UnaryPlusOp -> true
    | _ -> false

let isBinaryOperator token = 
    // not (isUnaryOperator token) will not work since Integer _ is neither
    match token with
    | BinPlusOp | BinMinOp | BinMulOp | BinDivOp | BinExpOp -> true
    | _ -> false

// infix evaluator
let reduceExpression() = 
    let extractValue t =
        match t with
        | Integer i -> i
        | _ -> failwithf "Unsupported %A" t

    let op = operators.Pop()
    if op = UnaryMinOp then
        let operand = operands.Pop() |> extractValue
        pushOperand (Integer -operand)
    else if op = UnaryPlusOp then operands.Pop() |> ignore
    else
        let right = operands.Pop() |> extractValue
        let left = operands.Pop() |> extractValue
        match op with
        | BinPlusOp -> pushOperand(Integer (left + right))
        | BinMinOp -> pushOperand(Integer (left - right))
        | BinMulOp -> pushOperand(Integer (left * right))
        | BinDivOp -> pushOperand(Integer (left / right))
        | BinExpOp -> pushOperand(Integer (Math.Pow(float left, float right) |> int))
        | _ -> failwithf "Unsupported operator %A" op

let parse tokens =
    let rec parse' (tokens: Token list) =
        match tokens with
        // read token
        | hd :: tl ->
            // if token is operand then push onto operand stack
            if isOperand hd then pushOperand hd

            // if token is unary prefix operator then push onto operator stack
            if isUnaryOperator hd then pushOperator hd

            // if token is binary operator, o1, then
            else if isBinaryOperator hd then
                // while operator token, o2, at top of operator stack, and
                //     either o1 is left-associative and its precedence is <= to that of o2
                //         or o1 is right-associative and its precedence is < that of o2
                //   reduce expression
                // push o1 onto the operator stack
                while operators.Count > 0 && isBinaryOperator(operators.Peek()) &&
                      (associativity |> Map.find hd = Left &&
                       precedense |> Map.find hd <= (precedense |> Map.find (operators.Peek())) ||
                       associativity |> Map.find hd = Right &&
                       precedense |> Map.find hd < (precedense |> Map.find (operators.Peek()))) do
                    reduceExpression()
                pushOperator hd
            
            // if token is left paren then push it onto operator stack
            else if hd = LParen then pushOperator hd

            // if token is right paren
            else if hd = RParen then
                // until token at top of operator stack is left paren
                while operators.Count > 0 && operators.Peek() <> LParen do 
                    reduceExpression()

                // if operator stack runs out without finding left paren then mismatched parens
                if operators.Count = 0 then failwith "Unmatched parenthesis"

                // pop left paren from stack
                if operators.Peek() = LParen then (operators.Pop() |> ignore)

            parse' tl
        | _ -> ()

    // while tokens to be read
    parse' tokens

    // when no more tokens to readand
    // while still tokens on operator stack
    while operators.Count > 0 do
        // if operator token on top of stack is paren then mismatched parentheses
        if operators.Peek() = LParen || operators.Peek() = RParen then
            failwith "Unmatched parenthesis"

        // reduce expression
        reduceExpression()

    // pop result of operand stack
    operands.Pop()

test <@ "1" |> toArray |> tokenize |> parse = Integer 1 @>
test <@ "-1" |> toArray |> tokenize |> parse = Integer -1 @>
test <@ "1+2" |> toArray |> tokenize |> parse = Integer 3 @>
test <@ "(1+2)" |> toArray |> tokenize |> parse = Integer 3 @>
test <@ "1+-2" |> toArray |> tokenize |> parse = Integer -1 @>
test <@ "-(1+2)" |> toArray |> tokenize |> parse = Integer -3 @>
test <@ "2*3" |> toArray |> tokenize |> parse = Integer 6 @>
test <@ "10/2" |> toArray |> tokenize |> parse = Integer 5 @>
test <@ "2^3^2" |> toArray |> tokenize |> parse = Integer 512 @>
test <@ "1+2*3" |> toArray |> tokenize |> parse = Integer 7 @>
test <@ "4^5/1+2*3" |> toArray |> tokenize |> parse = Integer -32 @>

module Program =
    [<EntryPoint>]
    let main _ =
        0