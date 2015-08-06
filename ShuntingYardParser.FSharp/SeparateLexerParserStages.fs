#if INTERACTIVE
#r "../packages/Unquote.2.2.2/lib/net40/Unquote.dll"
#r "System.dll"
#else
module SeparateLexerParserStages
#endif

open System
open Swensen.Unquote

// Backus-Naus expression grammar (whitespace-handling excluded)
// Not used directly for this lexer/parser because the parser
// combines parsing and evaluation using the Shunting Yard algorithm:
// 
// Expression := Term (BinOp Term)*
// Term := Integer | UnaryOp Term | '(' Expression ')'
// BinOps := '+' | '-' | '*' | '/' | '^'
// UnaryOps := '-' | '+'
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
// to simplify future evaluation or complicate the evaluator.

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

// val toArray : s:string -> char list
let toArray (s: string) = s.ToCharArray() |> List.ofArray

test <@ toArray "" = [] @>
test <@ toArray "1" = ['1'] @>
test <@ toArray "1+2" = ['1'; '+'; '2'] @>

// val parseDigit : _arg1:char list -> (int * char list) option
let parseDigit = function
    | hd :: tl ->
        match hd with
        |'0'|'1'|'2'|'3'|'4'
        |'5'|'6'|'7'|'8'|'9' as digit -> 
            Some (Char.GetNumericValue(digit) |> int, tl)
        | _ -> None
    | _ -> None

test <@ parseDigit ("" |> toArray) = None @>
test <@ parseDigit ("." |> toArray) = None @>
test <@ parseDigit ("2" |> toArray) = Some(2, []) @>
test <@ parseDigit ("12" |> toArray) = Some(1, ['2']) @>
test <@ parseDigit ("123" |> toArray) = Some(1, ['2'; '3']) @>

// val parseInteger : input:char list -> (Token * char list) option
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
test <@ parseInteger ("12.3" |> toArray) = Some(Integer 12, ['.'; '3']) @>

// val parseBinOp : _arg1:char list -> (Token * char list) option
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

// val parseParen : _arg1:char list -> (Token * char list) option
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

// val parseUnaryOp : input:char list -> (Token * char list) option
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

// val parseExpression : input:string -> Token list
let parseExpression input =
    // some types of tokens requires input to be consumed more
    // greedy than others. Try the most greedy one first to
    // resolve any ambiguity that might arise.

    // val parse' : Token list -> char list -> Token list
    let rec parse' tokens rest =
        match parseInteger rest with 
        | Some (integer, tl) -> parse' (integer :: tokens) tl
        | _ ->
            match parseUnaryOp rest with
            | Some (unaryOp, tl) -> 
                let isPrevTokenBinaryOp token = 
                    [BinPlusOp; BinMinOp; BinMinOp; BinDivOp; BinExpOp; LParen] 
                    |> List.exists (fun o -> o = token)
                if List.length tokens = 0 ||
                   isPrevTokenBinaryOp (List.head tokens) then
                    parse' (unaryOp :: tokens) tl
                else 
                    // No? Must be BinPlusOp or BinMinOp instead then
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
        
    parse' [] (input |> toArray) |> List.rev

test <@ parseExpression "" = [] @>
test <@ parseExpression "1" = [Integer 1] @>
test <@ parseExpression "1+2" = [Integer 1; BinPlusOp; Integer 2] @>
test <@ parseExpression "1-2" = [Integer 1; BinMinOp; Integer 2] @>

// is this legal math syntax? PowerShell can only parse "1-(-2)"
// whereas LibreOffice parses and evaluates supports "1--2"
test <@ parseExpression "1--2" = 
            [Integer 1; BinMinOp; UnaryMinOp; Integer 2] @>
test <@ parseExpression "-1" = [UnaryMinOp; Integer 1] @>
test <@ parseExpression "+1" = [UnaryPlusOp; Integer 1] @>
test <@ parseExpression "1+-2" = 
            [Integer 1; BinPlusOp; UnaryMinOp; Integer 2] @>
test <@ parseExpression "-(1+2*3/4^5)" =
            [UnaryMinOp; LParen; Integer 1; BinPlusOp; Integer 2; BinMulOp; 
             Integer 3; BinDivOp; Integer 4; BinExpOp; Integer 5; RParen] @>

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
let parseExpression' tokens =
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

test <@ "1" |> parseExpression |> parseExpression' = Integer 1 @>
test <@ "-1" |> parseExpression |> parseExpression' = Integer -1 @>
test <@ "1+2" |> parseExpression |> parseExpression' = Integer 3 @>
test <@ "(1+2)" |> parseExpression |> parseExpression' = Integer 3 @>
test <@ "1+-2" |> parseExpression |> parseExpression' = Integer -1 @>
test <@ "-(1+2)" |> parseExpression |> parseExpression' = Integer -3 @>
test <@ "2*3" |> parseExpression |> parseExpression' = Integer 6 @>
test <@ "10/2" |> parseExpression |> parseExpression' = Integer 5 @>
test <@ "2^3^2" |> parseExpression |> parseExpression' = Integer 512 @>
test <@ "1+2*3" |> parseExpression |> parseExpression' = Integer 7 @>
test <@ "4^5/1+2*3" |> parseExpression |> parseExpression' = Integer 1030 @>

module Program =
    [<EntryPoint>]
    let main _ =
        let lexemes = "1--2" |> parseExpression
        let result = lexemes |> parseExpression'
        0