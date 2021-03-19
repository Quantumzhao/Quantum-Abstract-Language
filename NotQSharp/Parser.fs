module Parser

open TokDef
open TypeDef
open Helper

(*  Expr ::= 
    | Let_Var
    | Let_Fun
    | Match
    | Apply
    | Structure

    Structure ::=
    | Tuple
    | Atom

    Atom ::=
    | Integer
    | Unit
    | Variable
    | Qubit
*)

let rec parse_let tokens : Expr * Token list =
    let rec parse_params (finished: string list)  (tokens: Token list) 
        : (string list * Token list) =
        match tokens with
        | Identifier id :: rest -> 
            let finished', rem = parse_params finished rest
            id :: finished', rem
        // base case, 
        // an equal marks the end of parameters
        | Equal :: after_equals -> finished, after_equals
        | redundant -> syntax_err Equal redundant
    // ---| Starts here |---
    match tokens with
    // if it is in the form let x = ...
    // which is a let binding to value
    | Let :: Identifier id :: Equal :: rest -> 
        // body_expr: the expression right after "="; the binded expression
        // after_body: remaining tokens after parsing the "let x = ..." part
        let body_expr, after_body = parse rest
        match after_body with
        | In :: after_in -> 
            // in_expr: the expression right after "in"
            // rest: the unused yokens
            let in_expr, rest = parse after_in
            Let_Var(id, body_expr, in_expr), rest
        | other -> syntax_err In other
    // if it is let function x y z = ...
    // a let binding to function
    | Let :: Identifier id :: rest -> 
        // ps: parameters
        // rest: rest of the tokens
        let ps, after_params = parse_params [] rest
        // similar to line 24,
        // same below
        let body_expr, after_body = parse after_params
        match after_body with
        | In :: after_in -> 
            let in_expr, rest = parse after_in
            Let_Fun(id, ps, body_expr, in_expr), rest
        | other -> syntax_err In other
    | other -> syntax_err Let other

and parse_match tokens =
    // parse the condition: match xxx with
    // assert if it is a match block, and consume these tokens
    let parse_condition tokens : (Expr * Token list) = 
        match tokens with
        | TokDef.Match :: rest -> 
            // expr: the expression wrapped in "match ... with"
            // rest': every token after this expression, including "With"
            //        i.e. [With] ...
            let expr, rest' = parse rest
            match rest' with
            // if rest' is in the correct format, i.e. starting with "With", 
            // return the condition expression together with all remaining tokens after "With"
            | With :: rest'' -> expr, rest''
            | other -> syntax_err With other
        | other -> syntax_err TokDef.Match other
    // parse a single case, including the pattern and corresponding branch
    // assume the token "|" has already been consumed
    let parse_case tokens =
        // parse the patterns, the part before "->"
        // e.g. a, b, c or _, _, a
        // finished: finished tokens
        // tokens: tokens to be parsed
        let rec parse_patterns finished tokens =
            match tokens with
            | sth :: Comma :: rest -> 
                // finished': every pattern that as been parsed; everything after this one
                // rest': all tokens left after parsing the 1st case
                let finished', rest' = parse_patterns finished rest
                match sth with
                | Identifier id -> (Placeholder id) :: finished', rest'
                | Underline -> WildCard :: finished', rest'
                | TokDef.Integer i -> (Int_Lit i) :: finished', rest'
                | Decimal d -> (Comp_Lit d) :: finished', rest'
                | other -> syntax_err "Identifier or Underline" other
            // the base case
            | sth :: Arrow :: rest -> 
                match sth with
                | Identifier id -> (Placeholder id) :: finished, rest
                | Underline -> WildCard :: finished, rest
                | TokDef.Integer i -> (Int_Lit i) :: finished, rest
                | Decimal d -> (Comp_Lit d) :: finished, rest
                | other -> syntax_err "Identifier or Underline" other
            | Arrow :: _ -> syntax_err "at least 1 pattern" Arrow
            | other -> syntax_err "some patterns" other
        // ----| parse_case starts here |----
        match tokens with
        | VBar :: rest -> 
            let patterns, rest = parse_patterns [] rest
            let expr, unused = parse rest
            (patterns, expr), unused
        | other -> syntax_err VBar other
    // parse all cases recursively
    let rec parse_cases finished tokens =
        match tokens with
        | VBar :: _ -> 
            // the first seen case
            // case: the first case
            // rest: all tokens left after parsing the 1st case
            let case, rest = parse_case tokens
            // cases: all cases after the 1st case
            // rest': all tokens after parsing all cases
            //        i.e. tokens left unused after parsing the whole match block
            let cases, rest' = parse_cases finished rest
            case :: cases, rest'
        | _ -> finished, tokens
    // ----| starts here |----
    let cond, after_cond = parse_condition tokens
    let cases, unused = parse_cases [] after_cond
    Match(cond, cases), unused

and parse_integer tokens =
    match tokens with
    | TokDef.Integer i :: rest -> Integer i, rest
    | other -> syntax_err TokDef.Integer other

and parse_decimal tokens =
    match tokens with
    // in this case, the complex is just r*e^0
    | Decimal d :: rest -> Complex(d, 0m), rest
    | other -> syntax_err Decimal other

/// parse any expression related to parenthesis,
/// also include apply and tuple
and parse_apply_tuple tokens =
    let expr, rest = parse tokens
    match rest with
    | Comma :: remain -> parse_tuple expr remain
    | _ -> parse_apply expr rest

and parse_tuple first_xp tokens =
    let rec parse_tuple_more finished tokens =
        let item, rest = parse tokens
        match rest with
        | Comma :: _ -> 
            let items, rest' = parse_tuple_more finished rest
            item :: items, rest'
        | RParen :: _ -> item :: finished, rest
        | other -> syntax_err Comma other
    let finished, unused = parse_tuple_more [] tokens
    Tuple(first_xp :: finished), unused

and parse_apply first_xp tokens =
    let rec parse_apply_args finished tokens =
        let arg, rest = parse tokens
        match rest with
        | Identifier _ :: _
        | TokDef.Integer _ :: _
        | Decimal _ :: _
        | LParen :: _ -> 
            let args, rest' = parse_apply_args finished rest
            arg :: args, rest'
        | _ -> arg :: finished, rest
    let finished, unused = parse_apply_args [] tokens
    Apply(first_xp, finished), unused

and parse_paren tokens =
    let expr, rest = parse_apply_tuple tokens
    match rest with
    | RParen :: unused -> expr, unused
    | other -> syntax_err RParen other

/// the main parse function
and parse tokens : Expr * Token list =
    match tokens with
    // there is no expression
    | [] -> (Unit, [])
    | LParen :: rest -> parse_paren rest
    | Let :: _ -> parse_let tokens
    | TokDef.Match :: _ -> parse_match tokens
    | TokDef.Integer _ :: _ -> parse_integer tokens
    | Decimal _ :: _ -> parse_decimal tokens
    | Identifier id :: rest -> Variable id, rest
    | _ -> parse_apply_tuple tokens

