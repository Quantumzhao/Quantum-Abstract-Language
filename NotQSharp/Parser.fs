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
        | Equal :: _ -> finished, tokens
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
    not_implemented_err ()

and parse_integer tokens =
    match tokens with
    | TokDef.Integer i :: rest -> Integer i, rest
    | other -> syntax_err TokDef.Integer other

and parse_decimal tokens =
    match tokens with
    // in this case, the complex is just r*e^0
    | Decimal d :: rest -> Complex(d, 0m), rest
    | other -> syntax_err Decimal other

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

