// Learn more about F# at http://fsharp.org

open Microsoft.Quantum.Simulation.Core
open Microsoft.Quantum.Simulation.Simulators
open System
open System.IO
open Lexer

/// given a list of tokens, outputs a complete list(string) of the tokens
let pretty_print tokens = 
    List.fold (fun acc t -> acc + t.ToString() + " ") "" tokens

[<EntryPoint>]
let main argv =
    let dir = Directory.GetCurrentDirectory()
    let out = lexer (dir + "/../Example/let")
    Console.WriteLine "Hello"
    Console.WriteLine (pretty_print out)
    0 // return an integer exit code
