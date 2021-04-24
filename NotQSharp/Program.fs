﻿// Learn more about F# at http://fsharp.org

open Microsoft.Quantum.Simulation.Core
open Microsoft.Quantum.Simulation.Simulators
open System
open System.IO
open Lexer
open TypeDef
open Parser
open Helper
open Interpreter


[<EntryPoint>]
let main argv =
    let dir = Directory.GetCurrentDirectory()
    let tokens = lexer (dir + "/../Example/high order")
    let expr, _ = parse tokens
    use sim = new QuantumSimulator()
    // Console.WriteLine "Hello"
    Console.WriteLine(pretty_print tokens)
    Console.WriteLine(pretty_draw expr)
    try
        let res = interp [] sim expr
        Console.WriteLine res
        0
    with
    | e -> 
        Console.WriteLine e.Message
        0
