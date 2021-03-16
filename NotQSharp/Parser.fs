module Parser

open TokDef
open TypeDef
open Helper

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
    let parse_condition tokens = 
        not_implemented_err ()
    // parse a single case, including the pattern and corresponding branch
    let parse_case tokens =
        not_implemented_err ()
    // parse all cases recursively
    let rec parse_cases finished tokens =
        // the first seen case
        // case: the first case
        // rest: all tokens left after parsing the 1st case
        let case, rest = parse_case tokens
        // cases: all cases after the 1st case
        // rest': all tokens after parsing all cases
        //        i.e. tokens left unused after parsing the whole match block
        let cases, rest' = parse_cases finished rest
        case :: cases, rest'
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
and parse_apply_tuple_w_paren tokens =
    not_implemented_err ()

and parse_tuple first_xp tokens =
    not_implemented_err ()

/// the main parse function
and parse tokens : Expr * Token list =
    match tokens with
    // there is no expression
    | [] -> (Unit, [])
    | Let :: _ -> parse_let tokens
    | TokDef.Match :: _ -> parse_match tokens
    | TokDef.Integer _ :: _ -> parse_integer tokens
    | Decimal _ :: _ -> parse_decimal tokens
    | Identifier id :: rest -> Variable id, rest
    | _ -> not_implemented_err ()

