module Lexer

open System
open System.IO
open System.Text.RegularExpressions
open TokDef

/// <summary>
/// cut the src code file into fragments according to space characters
/// </summary>
/// <param name="path">path of the file</param>
let get_code path = File.ReadAllText(path)

/// <summary>
/// tokenize the input program string
/// </summary>
/// <param name ="inputraw"> the input string</param>
let tok inputraw =
    /// <summary>
    /// tokenize helper with a wait to be consumed string and current position
    /// </summary>
    /// <param name ="input"> the input string that will wait to be further tokenized</param>
    /// <param name ="pos"> the position with repect to inputraw that is now tokenizing</param>
    let rec tokenize input pos =
        // Done tokenizing
        if pos >= (String.length inputraw) then
            []
        else
            // Generate regular expression of the Tokens
            // Tokens with unknown length will be matched with regex anchoring the beginning of the input string
            // Tokens with known length will be matched with specific position(which will always be zero that is from the beginning of the input) and specific length
            let Space_m =
                Regex("[ \n\r\x0c\t]+").Match(input, 0, 1)

            let Decimal_m = Regex("^-?[0-9]+\.[0-9]+").Match(input)

            let Integer_m = Regex("^-?[0-9]+").Match(input)

            let Mis_m =
                Regex("^[a-zA-Z_][a-zA-Z0-9_']*").Match(input)

            let LParen_m = Regex("\\(").Match(input, 0, 1)
            let RParen_m = Regex("\\)").Match(input, 0, 1)
            let VBar_m = Regex("\\|").Match(input, 0, 1)
            let Underline_m = Regex("_").Match(input, 0, 1)
            let Arrow_m = Regex("^->").Match(input)
            let Equal_m = Regex("=").Match(input, 0, 1)
            let Comma_m = Regex(",").Match(input, 0, 1)
            let Comment_m = Regex("^\/\/\s*\S+.*?\n").Match(input)
            let String_m = Regex("^\".*\"").Match(input)


            // Tokenizing string recursively.
            // After token has been matched, the matched string will be consumed, and the rest of the input string will be passed to the next recursion
            if (Space_m.Success) then
                tokenize (input.Substring(1)) (pos + 1)
            elif (Decimal_m.Success) then
                if (Decimal_m.Value.[0].Equals("-")) then 
                    Decimal((decimal 0) - (decimal Decimal_m.Value))
                    :: (tokenize (input.Substring(Decimal_m.Value.Length)) (pos + Decimal_m.Value.Length)) 
                else
                    Decimal(decimal Decimal_m.Value)
                    :: (tokenize (input.Substring(Decimal_m.Value.Length)) (pos + Decimal_m.Value.Length))
            elif (Integer_m.Success) then
                if (Integer_m.Value.[0].Equals("-")) then 
                    (Integer(0 - (int Integer_m.Value)))
                    :: (tokenize (input.Substring(Integer_m.Value.Length)) (pos + Integer_m.Value.Length))
                else
                    (Integer(int Integer_m.Value))
                    :: (tokenize (input.Substring(Integer_m.Value.Length)) (pos + Integer_m.Value.Length))
            elif (Mis_m.Success) then
                // Tokens with alphabets, it will otherwise be Identifier
                if Mis_m.Value.Equals("let")
                   && (Mis_m.Value.Length.Equals(3)) then
                    Let :: (tokenize (input.Substring(3)) (pos + 3))
                elif Mis_m.Value.Equals("in")
                     && (Mis_m.Value.Length.Equals(2)) then
                    In :: (tokenize (input.Substring(2)) (pos + 2))
                elif Mis_m.Value.Equals("match")
                     && (Mis_m.Value.Length.Equals(5)) then
                    Match :: (tokenize (input.Substring(5)) (pos + 5))
                elif Mis_m.Value.Equals("with")
                     && (Mis_m.Value.Length.Equals(4)) then
                    With :: (tokenize (input.Substring(4)) (pos + 4))
                else
                    (Identifier Mis_m.Value)
                    :: (tokenize (input.Substring(Mis_m.Value.Length)) (pos + Mis_m.Value.Length))
            elif (LParen_m.Success
                  && LParen_m.Value.Length.Equals(1)) then
                LParen
                :: (tokenize (input.Substring(1)) (pos + 1))
            elif (RParen_m.Success
                  && RParen_m.Value.Length.Equals(1)) then
                RParen
                :: (tokenize (input.Substring(1)) (pos + 1))
            elif (VBar_m.Success && VBar_m.Value.Length.Equals(1)) then
                VBar :: (tokenize (input.Substring(1)) (pos + 1))
            elif (Underline_m.Success
                  && Underline_m.Value.Length.Equals(1)) then
                Underline
                :: (tokenize (input.Substring(1)) (pos + 1))
            elif (Arrow_m.Success && Arrow_m.Value.Length.Equals(2)) then
                Arrow :: (tokenize (input.Substring(2)) (pos + 2))
            elif (Equal_m.Success && Equal_m.Value.Length.Equals(1)) then
                Equal :: (tokenize (input.Substring(1)) (pos + 1))
            elif (Comma_m.Success && Comma_m.Value.Length.Equals(1)) then
                Comma :: (tokenize (input.Substring(1)) (pos + 1))
            elif (Comment_m.Success) then
                (tokenize (input.Substring(Comment_m.Value.Length)) (pos + Comment_m.Value.Length))
            elif (String_m.Success) then
                // Take the string inside "",  that is from the index 1 of the string to the last second
                String(String_m.Value.[1..(String_m.Value.Length - 2)]) :: (tokenize (input.Substring(String_m.Value.Length)) (pos + String_m.Value.Length))
            else
                // The error message could only return the position of which character has matching issue, might need further ammendement
                failwith (pos.ToString())

    tokenize inputraw 0

/// <summary>maps each string to a token</summary>
/// <param name="path">path of the src code file</param>
let lexer path =
    let words = get_code path in
    tok words
