namespace QAL.Interpreter

open Microsoft.Quantum.Simulation.Simulators
open System
open System.IO
open Lexer
open Parser
open Interpreter

module Entry =

    [<EntryPoint>]
    let main argv =
        let dir = Directory.GetCurrentDirectory()
        if argv.Length = 0 then
            Console.WriteLine "REPL mode has not been implemented yet! "
            0
        else
            let tokens = lexer (dir + argv.[0])
            let expr, _ = parse tokens
            use sim = new QuantumSimulator()
            // Console.WriteLine(pretty_print tokens)
            // Console.WriteLine(pretty_draw expr)
            try
                let res = interp [] sim expr
                Console.WriteLine res
                0
            with
            | e -> 
                Console.WriteLine e.Message
                0
