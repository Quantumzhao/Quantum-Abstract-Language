module Lexer

open System
open System.IO
open System.Text.RegularExpressions
open TokDef
open Helper

let format_err id =
    failwith $"invalid format {id}"

/// <summary>
/// cut the src code file into fragments according to space characters
/// </summary>
/// <param name="path">path of the file</param>
let get_code path =
    File.ReadAllText(path).Split([|' '; '\n'; '\t'; '\r' |], StringSplitOptions.RemoveEmptyEntries)

/// <summary>
/// try to convert a string to either a number or an id. 
/// Source: https://markhneedham.com/blog/2009/05/10/f-regular-expressionsactive-patterns/
/// </summary>
/// <param name="string">the string to be matched</param>
/// <exception cref="System.Exception">throws exception when not in correct format</exception>
let match_num_id string =
    // F# active pattern, returns the first match if there is any
    // otherwise return none
    let (|Match|_|) pattern input =
        let m = Regex.Match(input, pattern) in
        if m.Success then
            Some(m.Groups.[0].Value)
        else
            None
    // match th numerals or ids
    match string with
    | Match "[0-9]+\.[0-9]+" string -> Decimal (decimal string)
    | Match "[0-9]+" string -> Integer (int string)
    | Match "[a-z]+[A-Z]*[0-9]*" string -> Identifier string
    | _ -> failwith (format_err string)

/// just match rules
let match_rule word =
    match word with
    | "(" -> LParen
    | ")" -> RParen
    | "let" -> Let
    | "in" -> In
    | "match" -> Match
    | "with" -> With
    | "|" -> VBar
    | "_" -> Underline
    | "->" -> Arrow
    | "=" -> Equal
    | "," -> Comma
    | _ -> match_num_id word

/// <summary>maps each string to a token</summary>
/// <param name="path">path of the src code file</param>
let lexer path =
    let words = get_code path
    List.map match_rule (arr_2_lst words)
