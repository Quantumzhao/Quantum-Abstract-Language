module Lexer

open System
open System.IO
open System.Text.RegularExpressions
open TokDef
open Helper

let format_err id = failwith $"invalid format {id}"

/// <summary>
/// cut the src code file into fragments according to space characters
/// </summary>
/// <param name="path">path of the file</param>
let get_code path = File.ReadAllText(path)
// File.ReadAllText(path).Split([|' '; '\n'; '\t'; '\r' |], StringSplitOptions.RemoveEmptyEntries)

// let (|Match|_|) pattern input =
//     let m = Regex(pattern).Match(input) in

//     if m.Success then
//         Some(List.tail [ for x in m.Groups -> x.Value ])
//     else
//         None
// match th numerals or ids


let rec tokenize input pos =
    if pos >= (String.length input) then
        []
    else
        let Space_m =
            Regex("[ \n\r\x0c\t]+").Match(input, pos, 1)

        let Decimal_m =
            Regex("[0-9]+\.[0-9]+").Match(input, pos)

        let Integer_m = Regex("[0-9]+").Match(input, pos)

        let Mis_m =
            Regex("[a-zA-Z][a-zA-Z0-9]*").Match(input, pos)

        let LParen_m = Regex("\\(").Match(input, pos, 1)
        let RParen_m = Regex("\\)").Match(input, pos, 1)
        let VBar_m = Regex("\\|").Match(input, pos, 1)
        let Underline_m = Regex("_").Match(input, pos, 1)
        let Arrow_m = Regex("->").Match(input, pos, 2)
        let Equal_m = Regex("=").Match(input, pos, 1)
        let Comma_m = Regex(":").Match(input, pos, 1)



        if (Space_m.Success) then
            tokenize input (pos + 1)
        elif (Decimal_m.Success) then
            let str = Decimal_m.Value

            (Decimal(Convert.ToDecimal Decimal_m.Value))
            :: (tokenize input (pos + Decimal_m.Value.Length))
        elif (Integer_m.Success) then
            (Integer(Int32.Parse(Integer_m.Value)))
            :: (tokenize input (pos + Integer_m.Value.Length))
        elif (Mis_m.Success) then
            if Mis_m.Value.Equals("let")
               && (Mis_m.Value.Length.Equals(3)) then
                Let :: (tokenize input (pos + 3))
            elif Mis_m.Value.Equals("in")
                 && (Mis_m.Value.Length.Equals(2)) then
                In :: (tokenize input (pos + 2))
            elif Mis_m.Value.Equals("match")
                 && (Mis_m.Value.Length.Equals(5)) then
                Match :: (tokenize input (pos + 5))
            elif Mis_m.Value.Equals("with")
                 && (Mis_m.Value.Length.Equals(4)) then
                With :: (tokenize input (pos + 4))
            else
                (Identifier Mis_m.Value)
                :: (tokenize input (pos + Mis_m.Value.Length))
        elif (LParen_m.Success
              && LParen_m.Value.Length.Equals(1)) then
            LParen :: (tokenize input (pos + 1))
        elif (RParen_m.Success
              && RParen_m.Value.Length.Equals(1)) then
            RParen :: (tokenize input (pos + 1))
        elif (VBar_m.Success && VBar_m.Value.Length.Equals(1)) then
            VBar :: (tokenize input (pos + 1))
        elif (Underline_m.Success
              && Underline_m.Value.Length.Equals(1)) then
            Underline :: (tokenize input (pos + 1))
        elif (Arrow_m.Success && Arrow_m.Value.Length.Equals(2)) then
            Arrow :: (tokenize input (pos + 2))
        elif (Equal_m.Success && Equal_m.Value.Length.Equals(1)) then
            Equal :: (tokenize input (pos + 1))
        elif (Comma_m.Success && Comma_m.Value.Length.Equals(1)) then
            Comma :: (tokenize input (pos + 1))
        else
            failwith (pos.ToString())
// (format_err "pos+" pos)

/// just match rules
// let match_rule word =
//     match word with
//     | "(" -> LParen
//     | ")" -> RParen
//     | "let" -> Let
//     | "in" -> In
//     | "match" -> Match
//     | "with" -> With
//     | "|" -> VBar
//     | "_" -> Underline
//     | "->" -> Arrow
//     | "=" -> Equal
//     | "," -> Comma
//     | _ -> match_num_id word

/// <summary>maps each string to a token</summary>
/// <param name="path">path of the src code file</param>
let lexer path =
    let words = get_code path
    tokenize words 0
// List.map match_rule (arr_2_lst words)
