// Learn more about F# at http://fsharp.org

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
    let tokens = lexer (dir + "/../Example/bell state")
    let expr, _ = parse tokens
    use sim = new QuantumSimulator()
    let res = interp [] sim expr
    // Console.WriteLine "Hello"
    Console.WriteLine (pretty_print tokens)
    Console.WriteLine (pretty_draw expr)
    Console.WriteLine res
    0 // return an integer exit code
