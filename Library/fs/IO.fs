namespace QAL.Library

open QAL.Definitions.TypeDef
open QAL.Utils.Error
open Arithmetic
open System

module IO =

    /// <summary>
    /// print the argument
    /// </summary>
    /// <param name="args">
    /// accepts: 
    /// - string
    /// - integer
    /// - complex (including decimals)
    /// - tuple
    /// </param>
    let rec print args =
        match args with
        | [] -> 
            Console.WriteLine () |> ignore
            Integer_Val 0
        | String_Val s :: tl -> 
            Console.Write s |> ignore
            print tl
        | Integer_Val i :: tl -> 
            Console.Write i |> ignore
            print tl
        | Complex_Val(m, a) :: tl -> 
            match complex_to_decimal (Complex_Val(m, a)) with
            // if it's a real number, print in real format
            | Some d -> 
                Console.Write d |> ignore
                print tl
            // if it's complex, print in r theta form
            | None -> 
                Console.Write $"{m}·e^{a}iπ" |> ignore
                print tl
        | Tuple_Val items :: [] -> 
            Console.Write "(" |> ignore
            let rec print_rec items =
                match items with
                | hd :: tl -> 
                    Console.Write $"{hd} "
                    print_rec tl
                | _ -> 
                    Console.WriteLine ")"
            (print_rec items) |> ignore
            Unit_Val
        | _ -> too_many_args_err 1 args
