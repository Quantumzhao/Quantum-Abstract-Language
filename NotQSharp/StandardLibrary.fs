module StandardLibrary

open Microsoft.Quantum.Simulation.Core
open Helper
open DecimalMath
open QuantumLib
open TypeDef
open Microsoft.Quantum.Canon

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
    // a QArray of Q# qubits
    let q_array = ClaimQubits.Run(sim, int64 n).Result
    // an F# list of my qubits
    let proc'ed_arr = List.map Qubit_Val (arr_2_lst q_array)
    // a system of my qubits
    System_Val proc'ed_arr

let new_qubit option sim =
    let qubit =
        match option with
        | Zero -> ClaimQubit.Run(sim, 0L).Result
        | One -> ClaimQubit.Run(sim, 1L).Result
        | Plus -> ClaimQubit.Run(sim, 2L).Result
        | Minus -> ClaimQubit.Run(sim, 3L).Result
    Qubit_Val qubit

// gives off a binary vector space of rank n
// n -> {0, 1}ⁿ
let bin_vec_space rank =
    let rec bin_vec_space_rec rank =
        if rank <= 0 then
            failwith $"illegal rank: {rank} must be greater than 0"
        elif rank = 1 then
            // the base case, consists of only single bit 1 or 0
            [[0]; [1]]
        else
            // suppose there is a subspace of rank n - 1
            // then add another degree of freedom to make it rank n
            let sub_space = bin_vec_space_rec (rank - 1)
            let group0 = List.map (fun v -> 0 :: v) sub_space
            let group1 = List.map (fun v -> 1 :: v) sub_space
            List.append group0 group1
    // code below is just: `int list list` to `Array(Array(Integer))`
    let int_list_to_Int_list lst =
        List.map Integer_Val lst
    let flist_to_my_list vec =
        Array_Val (List.map (fun v -> Array_Val (int_list_to_Int_list v)) vec)
    match rank with
    | Integer_Val i -> flist_to_my_list (bin_vec_space_rec i)
    | _ -> invalidArg "rank" "rank must be an integer"

