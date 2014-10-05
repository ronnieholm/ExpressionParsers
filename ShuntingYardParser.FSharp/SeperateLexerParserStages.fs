#if INTERACTIVE
#r "../packages/Unquote.2.2.2/lib/net40/Unquote.dll"
#r "System.dll"
#else
module SeperateLexerParserStages
#endif

open System
open Swensen.Unquote

// improvement:
//   store row, column, length of each token with each instance
//   parse and discard of whitespace
//   instead of first tokenizing the entire expression, have parser
//     call a next token function. The next token function could
//     start by removing any whitespace

// Backus-Naus expression grammar (whitespace-handling excluded)
// Not used directly for this lexer/parser because the parser
// combines parsing and evaluation using the Shunting Yard algorithm:
// 
// Expression := Term (BinOp Term)*
// Term := Integer | UnaryOp Term | '(' Expression ')'
// BinOps := '+' | '-' | '*' | '/' | '^'
// UnaryOps := '-'
// Digit := '0' | '1' | '2' | '3' | '4' | '5' | '6' | '7' | '8' | '9'
// Integer := Digit+

// all the top-level expressionParser requires is a stream of tokens. This 
// isn't typical of a parser. Instead a parser would normally construct an 
// abstract syntax tree from the tokens and pass the tree to the evaluator. 

// a parser for a real programming language would construct a syntax tree 
// of all the source code and either call upon a different parser for the 
// expression parts (to construct the correct trees given associativity and 
// precedence) or leave the expression trees deliberately "incorrect" and call
// upon different evaluators for expressions and statements. That way the 
// parser could still assert the source code was valid. It's a trade-off 
// between complicating the parser by having it do a bit of evaluation
// to simplify evaluation or complicate the evaluator.

// tokens act as a grouping mechanism of information within the
// domain we're parsing (mathematical expressions). Therefore,
// we don't require seperate types of tokens for each digit, but 
// treat them the same and represent them as a common Integer token 
// type. Not so with the operators which we want to treat differently 
// during parsing and evaluation. Note also how the UnaryMinOp and 
// BinMinOp are both represented by the "-" character in an expression, 
// but depending on its position (its contexts) it has different meaning.
// Here, the unary operators are mostly for illustration purposes.
// We could get rid of the unary operators and extend the integer parser
// to parse the sign as well.
type Token =
    | Integer of Int32
    | BinPlusOp
    | BinMinOp
    | BinMulOp
    | BinDivOp
    | BinExpOp
    | UnaryPlusOp
    | UnaryMinOp
    | LParen
    | RParen

// lexing is effectively parsing at the character level so we 
// break down the input into its atomic units (characters)
let toArray (s: string) = s.ToCharArray() |> List.ofArray

// all parsers attempt to consume characters from left to right
// until the parser can either return Some (the parsed characters
// and the remaining input) or the expected token couldn't be 
// constructed from the input.

// parses a single digit
let parseDigit = function
    | hd :: tl ->
        match hd with
        |'0'|'1'|'2'|'3'|'4'
        |'5'|'6'|'7'|'8'|'9' as digit -> 
            Some (Char.GetNumericValue(digit) |> int, tl)
        | _ -> None
    | _ -> None

test <@ parseDigit ("123" |> toArray) = Some(1, ['2'; '3']) @>
test <@ parseDigit ("12" |> toArray) = Some(1, ['2']) @>
test <@ parseDigit ("2" |> toArray) = Some(2, []) @>
test <@ parseDigit ("." |> toArray) = None @>
test <@ parseDigit ("" |> toArray) = None @>

// parses a series of digits into an integer
// could be replaced by a generic parser that keeps applying the
// parseDigit parser to the input until parseDigit would return None.
let parseInteger input =
    let rec parse' input digits =
        match parseDigit input with
        | Some (digit, rest) -> parse' rest (digit :: digits)
        | None -> (digits, input)

    let (digits, tl) = parse' input []
    let s = digits |> List.rev |> List.fold (fun acc d -> acc + d.ToString()) ""
    let (isInteger, integer) = Int32.TryParse(s)
    if isInteger then Some(Integer(integer), tl) else None

test <@ parseInteger ("" |> toArray) = None @>
test <@ parseInteger ("1" |> toArray) = Some(Integer 1, []) @>
test <@ parseInteger ("01" |> toArray) = Some(Integer 1, []) @>
test <@ parseInteger ("12" |> toArray) = Some(Integer 12, []) @>
test <@ parseInteger ("123" |> toArray) = Some(Integer 123, []) @>
test <@ parseInteger ("123.4" |> toArray) = Some(Integer 123, ['.'; '4']) @>

// parses binary operators
let parseBinOp = function
    | hd :: tl ->
        match hd with
        | '+' -> Some(BinPlusOp, tl)
        | '-' -> Some(BinMinOp, tl)
        | '*' -> Some(BinMulOp, tl)
        | '/' -> Some(BinDivOp, tl)
        | '^' -> Some(BinExpOp, tl)
        | _ -> None
    | _ -> None

test <@ parseBinOp ("" |> toArray) = None @>
test <@ parseBinOp ("1" |> toArray) = None @>
test <@ parseBinOp ("+" |> toArray) = Some(BinPlusOp, []) @>
test <@ parseBinOp ("-" |> toArray) = Some(BinMinOp, []) @>
test <@ parseBinOp ("*" |> toArray) = Some(BinMulOp, []) @>
test <@ parseBinOp ("/" |> toArray) = Some(BinDivOp, []) @>
test <@ parseBinOp ("^" |> toArray) = Some(BinExpOp, []) @>
test <@ parseBinOp ("^12" |> toArray) = Some(BinExpOp, ['1'; '2']) @>

// parses parenthesis
let parseParen = function
    | hd :: tl ->
        match hd with
        | '(' -> Some(LParen, tl)
        | ')' -> Some(RParen, tl)
        | _ -> None
    | _ -> None

test <@ parseParen ("" |> toArray) = None @>
test <@ parseParen ("1" |> toArray) = None @>
test <@ parseParen ("(" |> toArray) = Some(LParen, []) @>
test <@ parseParen (")" |> toArray) = Some(RParen, []) @>
test <@ parseParen (")+1" |> toArray) = Some(RParen, ['+'; '1']) @>

// we can only discriminate between UnaryOp and BinOp with a priory
// knowledge of number of tokens matched thus far and last token 
// matched. We don't provide the function with this info, but instead
// make sure we call parseUnaryOp at the right place from inside the
// parseExpression parser so it can infer the information. It would
// be simpler to treat the sign as part of the integer parser directly.
let parseUnaryOp input =  
    match input with
    | hd :: tl ->
        match hd with
        | '-' -> Some(UnaryMinOp, tl)
        | '+' -> Some(UnaryPlusOp, tl)
        | _ -> None
    | _ -> None

test <@ parseUnaryOp ("" |> toArray) = None @>
test <@ parseUnaryOp ("-" |> toArray) = Some(UnaryMinOp, []) @>
test <@ parseUnaryOp ("+" |> toArray) = Some(UnaryPlusOp, []) @>
test <@ parseUnaryOp ("+1" |> toArray) = Some(UnaryPlusOp, ['1']) @>

// top-level lexer
let tokenizeExpression input =
    // some types of tokens require the input to consumed more
    // greedy than others. We try the most greedy ones first to
    // make sure no ambiguity arise.
    let rec parse' tokens rest =
        match parseInteger rest with 
        | Some (integer, tl) -> parse' (integer :: tokens) tl
        | _ ->
            match parseUnaryOp rest with
            | Some (unaryOp, tl) -> 
                let ops = [BinPlusOp; BinMinOp; BinMinOp; BinDivOp; BinExpOp; LParen]
                if (List.length tokens = 0) || (ops |> List.exists (fun o -> o = (List.head tokens))) then
                    parse' (unaryOp :: tokens) tl
                else 
                    // we're mistaken -- backtrack. It must be either BinPlus or BinMinOp instead
                    match unaryOp with
                    | UnaryMinOp -> parse' (BinMinOp :: tokens) tl 
                    | UnaryPlusOp -> parse' (BinPlusOp :: tokens) tl
                    | _ -> failwith "Should never happen"
            | _ -> 
                match parseBinOp rest with
                | Some (binOp, tl) -> parse' (binOp :: tokens) tl
                | _ ->
                    match parseParen rest with
                    | Some (paren, tl) -> parse' (paren :: tokens) tl
                    | _ -> tokens
        
    parse' [] input |> List.rev

test <@ tokenizeExpression  ("" |> toArray) = [] @>
test <@ tokenizeExpression  ("1" |> toArray) = [Integer 1] @>
test <@ tokenizeExpression  ("1+2" |> toArray) = [Integer 1; BinPlusOp; Integer 2] @>
test <@ tokenizeExpression  ("1-2" |> toArray) = [Integer 1; BinMinOp; Integer 2] @>

// is this legal math syntax? PowerShell cannot parse this but requires "1-(-2)"
// whereas LibreOffice parses and evaluates it to 3
test <@ tokenizeExpression  ("1--2" |> toArray) = [Integer 1; BinMinOp; UnaryMinOp; Integer 2] @>
test <@ tokenizeExpression  ("-1" |> toArray) = [UnaryMinOp; Integer 1] @>
test <@ tokenizeExpression  ("+1" |> toArray) = [UnaryPlusOp; Integer 1] @>
test <@ tokenizeExpression  ("1+-2" |> toArray) = [Integer 1; BinPlusOp; UnaryMinOp; Integer 2] @>
test <@ tokenizeExpression  ("-(1+2*3/4^5)" |> toArray) = [UnaryMinOp; LParen; Integer 1; BinPlusOp; Integer 2; BinMulOp; Integer 3; BinDivOp; Integer 4; BinExpOp; Integer 5; RParen] @>

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

open System.Collections.Generic

type Associativity =
    | Left
    | Right

let operatorsInfo =
    [(Token.BinExpOp, 4, Right)
     (Token.UnaryMinOp, 3, Left)
     (Token.BinMulOp, 2, Left)
     (Token.BinDivOp, 2, Left)
     (Token.BinPlusOp, 1, Left)
     (Token.BinMinOp, 1, Left)]

let lookupOperator token = 
    operatorsInfo 
    |> List.find (fun (name, _, _) -> token = name) 
    |> fun (_, p, a) -> p, a

let operands = new Stack<Token>()
let operators = new Stack<Token>()

let pushOperand token = operands.Push(token)
let pushOperator token = operators.Push(token)

let isOperand = function
    | Integer _ -> true
    | _ -> false

let isUnaryOperator = function
    | UnaryMinOp | UnaryPlusOp -> true
    | _ -> false

let isBinaryOperator = function
    // not (isUnaryOperator token) will not work since Integer _ is neither
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

// depending on your definition of a parser, this one is either a Shunting 
// Yard expression parser or an evaluator. If you only wish to do one 
// thing with the input, such as evaluate the expression to an integer,
// the below code suffices. However, the Shunting Yard algorithm can
// also be used to construct syntax trees from tokens. The algorithm
// remains the same, but you must modify the reduceExpression function
// to not store integers on the operand stack but syntax tree nodes. The
// node that remains after all tokens have been evaluated in the syntax
// tree representing the expression.
let parseExpression tokens =
    // while tokens to be read
    let rec parse' (tokens: Token list) =
        match tokens with
        // read token
        | hd :: tl ->
            // if token is operand then push onto operand stack
            if isOperand hd then pushOperand hd

            // if token is unary prefix operator then push onto operator stack
            // todo: while not else if here?
            else if isUnaryOperator hd then pushOperator hd

            // if token is binary operator, o1, then
            else if isBinaryOperator hd then
                // while operator token, o2, at top of operator stack, and
                //     either o1 is left-associative and its precedence is <= to that of o2
                //         or o1 is right-associative and its precedence is < that of o2
                //   reduce expression
                // push o1 onto the operator stack
                let pO1, aO1 = lookupOperator hd
                let isReduceRequired operator =
                    let o2 = if isBinaryOperator(operator) then Some(lookupOperator(operator)) else None
                    match o2 with
                    | Some(pO2, aO2) -> (aO1 = Left && pO1 <= pO2) || (aO1 = Right && pO1 < pO2)                        
                    | None -> false

                while operators.Count > 0 && isReduceRequired(operators.Peek()) do 
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
        | [] ->
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

    parse' tokens

test <@ "1" |> toArray |> tokenizeExpression |> parseExpression = Integer 1 @>
test <@ "-1" |> toArray |> tokenizeExpression |> parseExpression = Integer -1 @>
test <@ "1+2" |> toArray |> tokenizeExpression |> parseExpression = Integer 3 @>
test <@ "(1+2)" |> toArray |> tokenizeExpression |> parseExpression = Integer 3 @>
test <@ "1+-2" |> toArray |> tokenizeExpression |> parseExpression = Integer -1 @>
test <@ "-(1+2)" |> toArray |> tokenizeExpression |> parseExpression = Integer -3 @>
test <@ "2*3" |> toArray |> tokenizeExpression |> parseExpression = Integer 6 @>
test <@ "10/2" |> toArray |> tokenizeExpression |> parseExpression = Integer 5 @>
test <@ "2^3^2" |> toArray |> tokenizeExpression |> parseExpression = Integer 512 @>
test <@ "1+2*3" |> toArray |> tokenizeExpression |> parseExpression = Integer 7 @>
test <@ "4^5/1+2*3" |> toArray |> tokenizeExpression |> parseExpression = Integer 1030 @>

module Program =
    [<EntryPoint>]
    let main _ =
        let lexemes = "1--2" |> toArray |> tokenizeExpression
        let result = lexemes |> parseExpression
        0