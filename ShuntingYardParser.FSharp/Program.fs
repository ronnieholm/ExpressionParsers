module ShuntingYardParser.FSharp


(*
http://stevegilham.blogspot.dk/2013/12/an-introduction-to-functional_11.html
let digitToint i =
   match i with
     '0' -> 0
   | '1' -> 1
   | '2' -> 2
   | '3' -> 3
   | '4' -> 4
   | '5' -> 5
   | '6' -> 6
   | '7' -> 7
   | '8' -> 8
   | '9' -> 9
   | (_) -> 0 ;;

let mkint s =
 let rec mkintAux s a =
     match s with
      [] -> a
    | (x::xs) ->  mkintAux xs ((a*10) + (digitToint x))
 mkintAux s 0;;
*)
open System

module Test =   
    // inspired from http://devhawk.net/2007/12/10/practical-parsing-in-f/

    // todo: implement space production!

    (*
        Expression := Term (BinaryOperator Term)*
        Term := Literal | LParen Expression RParen | UnaryOperator Term
        BinaryOperator := "+" | "-" | "*" | "/" | "^"
        UnaryOperator := "-"
        LParen := "("
        RParen := ")"
        Literal := "0" | "1" | ... | "9"
    *)

    type TokenType = // lexemes
        | Literal of int
        | LParen
        | RParen
        | BinaryAdd
        | BinarySub
        | BinaryMul
        | BinaryDiv
        | BinaryExp
        | UnaryMinus

    /// retrieves head of parse buffer, hiding underlying buffer data structure
    // val Head : input:'a list -> ('a * 'a list) option
    let (|Head|_|) input =      
        match input with
        | h :: t -> Some(h, t)
        | [] -> None

    /// checks to see if token is at the front of the parse buffer. If so, return
    /// the remainder of the buffer after token. If not, returns None
    // val consume : input:'a list -> 'a list option when 'a : equality
    let (|Token|_|) token input =
        let rec parseToken token input =
            match token, (|Head|_|) input with
            | h1 :: [], Some(h2, input) when h1 = h2 -> Some(input)
            | h1 :: t1, Some(h2, t2) when h1 = h2 -> parseToken t1 t2
            | _ -> None
        parseToken (List.ofSeq token) input

    // Expression := Term (BinaryOperator Term)*
    let rec (|Expression|_|) input =
        let rec (|BinaryOperatorTerm|_|) input =
            match input with
            | BinaryOperator (token, Term rest) ->
                match rest with
                | BinaryOperator _ -> (|BinaryOperatorTerm|_|) rest
                | _ -> Some(rest)
            | _ -> None     
        match input with
        | Term rest ->
            match rest with
            | BinaryOperatorTerm rest -> Some rest
            | _ -> Some rest
        | _ -> None

    // Term := Literal | LParen Expression RParen | UnaryOperator Term
    and (|Term|_|) = function
        | Literal (token, rest) -> Some rest
        | LParen (Expression (RParen rest)) -> Some rest
        | UnaryOperator (token, Term rest) -> Some rest
        | _ -> None

    // LParen := "("
    and (|LParen|_|) = function
        | Token "(" rest -> printfn "LParen"; Some rest
        | _ -> None

    // RParen := ")"
    and (|RParen|_|) = function
        | Token ")" rest -> printfn "RParen"; Some rest
        | _ -> None

    // Literal := "0" | "1" | ... | "9"
    and (|Literal|_|) = function
        | Token "0" rest -> Some (Literal 0, rest)
        | Token "1" rest -> Some (Literal 1, rest)
        | Token "2" rest -> Some (Literal 2, rest)
        | Token "3" rest -> Some (Literal 3, rest)
        | Token "4" rest -> Some (Literal 4, rest)
        | Token "5" rest -> Some (Literal 5, rest)
        | Token "6" rest -> Some (Literal 6, rest)
        | Token "7" rest -> Some (Literal 7, rest)
        | Token "8" rest -> Some (Literal 8, rest)
        | Token "9" rest -> Some (Literal 9, rest)
        | _ -> None // failwith "Expected literal"

    // UnaryOperator := "-"
    and (|UnaryOperator|_|) = function
        | Token "-" rest -> printfn "UnaryMinus"; Some (UnaryMinus, rest)
        | _ -> None

    // BinaryOperator := ”+” | ”-” | ”*” | ”/” | ”^”
    and (|BinaryOperator|_|) = function
        | Token "+" rest -> Some (BinaryAdd, rest)
        | Token "-" rest -> Some (BinarySub, rest)
        | Token "*" rest -> Some (BinaryMul, rest)
        | Token "/" rest -> Some (BinaryDiv, rest)
        | Token "^" rest -> Some (BinaryExp, rest)
        | _ -> None

    // val stringToParseBuffer : input:seq<'a> -> 'a list
    let stringToParseBuffer input = List.ofSeq input

module Program =
    open Test

    [<EntryPoint>]
    let main args =
        let x = (|Expression|_|) (stringToParseBuffer "1^2")
        printfn "%A" x
        0