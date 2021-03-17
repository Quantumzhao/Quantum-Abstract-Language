module StandardLibrary

open Microsoft.Quantum.Simulation.Core
open Helper
open DecimalMath
open QuantumLib
open TypeDef

// TODO: 
// map: the familiar map. (a' -> b') -> a' list -> b' list
// fori: the for iteration. (int -> a' -> (b' * c')) -> a' list -> (b' list)
// fold: the familiar fold. (a' -> b' -> a') -> a' -> b' list -> a'
// borrow: borrow a set of qubits, do some operations, and then return the qubits back to the set
//         (System -> a') -> System -> System * a'
// trim: remove all the empty holes in a qubits set left by any indexing or accessing operations
//       System -> System

let PI = Complex_Val(DecimalEx.Pi, 0m)

let E = Complex_Val(DecimalEx.E, 0m)

let I = Complex_Val(1m, DecimalEx.Pi)

let find_variable name =
    match name with
    | "PI" -> Some PI
    | "E" -> Some E
    | "I" -> Some I
    | _ -> None

let find_func name =
    match name with
    | _ -> None

let find_any name =
    match find_variable name with
    | Some value -> Some value
    | None -> 
        match find_func name with
        | Some value -> Some value
        | None -> None

let new_qubits n sim =
    let q_array = ClaimQubits.Run(sim, int64 n).Result
    let proc'ed_arr = List.map Qubit_Val (arr_2_lst q_array)
    System_Val proc'ed_arr

let new_qubit option sim =
    let qubit =
        match option with
        | Zero -> ClaimQubit.Run(sim, 0L).Result
        | One -> ClaimQubit.Run(sim, 1L).Result
        | Plus -> ClaimQubit.Run(sim, 2L).Result
        | Minus -> ClaimQubit.Run(sim, 3L).Result
    Qubit_Val qubit

