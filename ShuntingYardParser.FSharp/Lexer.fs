#if INTERACTIVE
#r "../packages/Unquote.2.2.2/lib/net40/Unquote.dll"
#r "System.dll"
#else
module Lexer
#endif

open System
open Swensen.Unquote

// tokens to shold be atomic unit, hence we don't have a
// Digit tokens as that's represented within an Integer token.
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

let toArray (s: string) = s.ToCharArray() |> List.ofArray

let parseDigit input =
    match input with
    | hd :: tl ->
        match hd with
        |'0'|'1'|'2'|'3'|'4'
        |'5'|'6'|'7'|'8'|'9' as digit -> 
            Some (tl, Char.GetNumericValue(digit) |> int)
        | _ -> None
    | _ -> None

test <@ parseDigit ("123" |> toArray) = Some(['2'; '3'], 1) @>
test <@ parseDigit ("12" |> toArray) = Some(['2'], 1) @>
test <@ parseDigit ("2" |> toArray) = Some([], 2) @>
test <@ parseDigit ("." |> toArray) = None @>
test <@ parseDigit ("" |> toArray) = None @>

let parseInteger input =
    let rec parse' input digits =
        match parseDigit input with
        | Some (rest, digit) -> parse' rest (digit :: digits)
        | None -> (input, digits)

    let (tl, digits) = parse' input []
    let s = digits |> List.rev |> List.fold (fun acc d -> acc + d.ToString()) ""
    let (isInteger, integer) = Int32.TryParse(s)
    if isInteger then Some (tl, Integer(integer)) else None

test <@ parseInteger ("" |> toArray) = None @>
test <@ parseInteger ("1" |> toArray) = Some([], Integer 1) @>
test <@ parseInteger ("12" |> toArray) = Some([], Integer 12) @>
test <@ parseInteger ("123" |> toArray) = Some([], Integer 123) @>
test <@ parseInteger ("123.4" |> toArray) = Some(['.'; '4'], Integer 123) @>

let parseBinOp input =
    match input with
    | hd :: tl ->
        match hd with
        | '+' -> Some (tl, BinPlusOp)
        | '-' -> Some (tl, BinMinOp)
        | '*' -> Some (tl, BinMulOp)
        | '/' -> Some (tl, BinDivOp)
        | '^' -> Some (tl, BinExpOp)
        | _ -> None
    | _ -> None

test <@ parseBinOp ("" |> toArray) = None @>
test <@ parseBinOp ("1" |> toArray) = None @>
test <@ parseBinOp ("+" |> toArray) = Some([], BinPlusOp)@>
test <@ parseBinOp ("-" |> toArray) = Some([], BinMinOp)@>
test <@ parseBinOp ("*" |> toArray) = Some([], BinMulOp)@>
test <@ parseBinOp ("/" |> toArray) = Some([], BinDivOp)@>
test <@ parseBinOp ("^" |> toArray) = Some([], BinExpOp)@>
test <@ parseBinOp ("^12" |> toArray) = Some(['1'; '2'], BinExpOp)@>

let parseParen input =
    match input with
    | hd :: tl ->
        match hd with
        | '(' -> Some (tl, LParen)
        | ')' -> Some (tl, RParen)
        | _ -> None
    | _ -> None

test <@ parseParen ("" |> toArray) = None @>
test <@ parseParen ("1" |> toArray) = None @>
test <@ parseParen ("(" |> toArray) = Some([], LParen) @>
test <@ parseParen (")" |> toArray) = Some([], RParen) @>
test <@ parseParen (")+1" |> toArray) = Some(['+'; '1'], RParen) @>

// can only discriminate between UnaryOp and BinOp with a priory
// knowledge about number of tokens matched and last token matched.
// We don't pass this information, but make sure we call it at
// at the proper place from inside the lexer. If it wasn't
// because it complicated the grammar unnecessarily this could
// actually be a production in itself
let parseUnaryOp input =  
    match input with
    | hd :: tl ->
        match hd with
        | '-' -> Some (tl, UnaryMinOp)
        | '+' -> Some (tl, UnaryPlusOp)
        | _ -> None
    | _ -> None

test <@ parseUnaryOp ("" |> toArray) = None @>
test <@ parseUnaryOp ("-" |> toArray) = Some ([], UnaryMinOp) @>
test <@ parseUnaryOp ("+" |> toArray) = Some ([], UnaryPlusOp) @>
test <@ parseUnaryOp ("+1" |> toArray) = Some (['1'], UnaryPlusOp) @>

let tokenize input =
    // some types of token are more greedy than other. Try the
    // most greedy ones first and make sure no ambiguity arise.
    // As long as the tokens are single-character, the order
    // doesn't matter but for integer and floats, we should
    // try floats first
    let rec tokenize' input tokens =
        match parseInteger input with 
        | Some (tl, integer) -> tokenize' tl (integer :: tokens)
        | _ ->
            match parseUnaryOp input with
            | Some (tl, unaryOp) -> 
                let ops = [BinPlusOp; BinMinOp; BinMinOp; BinDivOp; BinExpOp; LParen]
                if (List.length tokens = 0) || (ops |> List.exists (fun o -> o = (List.head tokens))) then
                    tokenize' tl (unaryOp :: tokens)
                else 
                    // we're mistaken. It must be either BinPlus or BinMinOp instead
                    match unaryOp with
                    | UnaryMinOp -> tokenize' tl (BinMinOp :: tokens)
                    | UnaryPlusOp -> tokenize' tl (BinPlusOp :: tokens)
                    | _ -> failwith "Should never happen"
            | _ -> 
                match parseBinOp input with
                | Some (tl, binOp) -> tokenize' tl (binOp :: tokens)
                | _ ->
                    match parseParen input with
                    | Some (tl, paren) -> tokenize' tl (paren :: tokens)
                    | _ -> tokens
        
    tokenize' input [] |> List.rev

// todo: ignore whitespace. Hard to due without dedicated next tokens
// method that all the parser functions utilize
test <@ tokenize ("" |> toArray) = [] @>
test <@ tokenize ("1" |> toArray) = [Integer 1] @>
test <@ tokenize ("1+2" |> toArray) = [Integer 1; BinPlusOp; Integer 2] @>
test <@ tokenize ("1+2" |> toArray) = [Integer 1; BinPlusOp; Integer 2] @>
test <@ tokenize ("-1" |> toArray) = [UnaryMinOp; Integer 1] @>
test <@ tokenize ("+1" |> toArray) = [UnaryPlusOp; Integer 1] @>
test <@ tokenize ("1+-2" |> toArray) = [Integer 1; BinPlusOp; UnaryMinOp; Integer 2] @>
test <@ tokenize ("-(1+2*3/4^5)" |> toArray) = [UnaryMinOp; LParen; Integer 1; BinPlusOp; Integer 2; BinMulOp; Integer 3; BinDivOp; Integer 4; BinExpOp; Integer 5; RParen] @>