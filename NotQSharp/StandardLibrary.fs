module StandardLibrary

open Microsoft.Quantum.Simulation.Core
open Helper
open DecimalMath
open QuantumLib
open TypeDef
open DecimalMath
open Microsoft.Quantum.Canon

// TODO: 
// map: the familiar map. (a' -> b') -> a' list -> b' list
// fold: the familiar fold. (a' -> b' -> a') -> a' -> b' list -> a'
// range: returns a range from some int to some int
// borrow: borrows a set of qubits, do some operations, and then return the qubits back to the set
//         (System -> a') -> System -> System * a'
// trim: remove all the empty holes in a qubits set left by any indexing or accessing operations
//       System -> System

let private too_many_args_err (expect: int) (actual_arg_list: Value list) =
    syntax_err $"{expect} args" $"{actual_arg_list.Length} args"

let PI = Complex_Val(DecimalEx.Pi, 0m)

let E = Complex_Val(DecimalEx.E, 0m)

let I = Complex_Val(1m, DecimalEx.Pi)

let find_variable name =
    match name with
    | "PI" -> Some PI
    | "E" -> Some E
    | "I" -> Some I
    | _ -> None

let private arg_real_check a = a = 0m || a = 1m || a = -1m

let add interp_w_env e1 e2 =
    let var1 = interp_w_env e1
    let var2 = interp_w_env e2
    match var1, var2 with
    | Integer_Val i1, Integer_Val i2 -> Integer_Val(i1 + i2)
    | Complex_Val(m1, a1), Complex_Val(m2, a2) -> 
        if a1 = 0m && a2 = 0m then
            Complex_Val (m1 + m2, 0m)
        else // why would anyone do this
            let argand_to_cartesian m a =
                m * DecimalEx.Cos (a * DecimalEx.Pi), m * DecimalEx.Sin (a * DecimalEx.Pi)
            let r1, i1 = argand_to_cartesian m1 a1
            let r2, i2 = argand_to_cartesian m2 a2
            let r = r1 + r2
            let i = i1 + i2
            let m = DecimalEx.Sqrt (r * r + i * i)
            let a = DecimalEx.ATan (i / r) / DecimalEx.Pi
            Complex_Val(m, a)
    | Complex_Val(m1, a1), Integer_Val i when arg_real_check a1 -> not_implemented_err ()
    | Integer_Val i, Complex_Val(m2, a2) when arg_real_check a2 -> not_implemented_err ()
    | other1, other2 -> 
        invalidArg "expression 1 or expression 2" $"cannot cast either {other1} or {other2} to integer"

let to_int interp_w_env c =
    match interp_w_env c with
    | Complex_Val(m, a) when arg_real_check a -> int (round m)
    | Complex_Val(_, _) -> 
        invalidArg "complex" "imaginary part is not 0: ambiguous rounding"
    | Integer_Val i -> i
    | other -> invalidArg "complex" $"cannot cast {other} to integer"

let to_complex interp_w_env num =
    match interp_w_env num with
    | Integer_Val i when i >= 0 -> Complex_Val(decimal i, 0m)
    | Integer_Val i when i < 0 -> Complex_Val(decimal i, 1m)
    | Complex_Val(m, a) -> Complex_Val(m, a)
    | other -> invalidArg "number" $"cannot cast {other} to integer"

// =============== Quantum Part ==================
// require refactor in the future

let private new_qubits arg sim =
    match arg with
    | Integer_Val n :: [] -> 
        // a QArray of Q# qubits
        let q_array = ClaimQubits.Run(sim, int64 n).Result
        // an F# list of my qubits
        let proc'ed_arr = List.map Qubit_Val (arr_2_lst q_array)
        // a system of my qubits
        System_Val proc'ed_arr
    | _ -> too_many_args_err 1 arg

let private new_qubit args sim =
    match args with
    | Integer_Val option :: [] -> 
        let qubit = ClaimQubit.Run(sim, int64 option).Result
        Qubit_Val qubit
    | _ -> too_many_args_err 1 args

let private measure args sim =
    match args with
    | Qubit_Val q :: [] -> 
        let res = Measure.Run(sim, q).Result
        if res = Result.One then Integer_Val 1
        else Integer_Val 0
    | _ -> too_many_args_err 1 args

let private basic_gate code arg sim =
    match arg with
    | Qubit_Val q :: [] -> 
        match code with
        | "H" -> Qubit_Val (Hadamard.Run(sim, q).Result)
        | "X" -> Qubit_Val (Pauli_X.Run(sim, q).Result)
        | "Y" -> Qubit_Val (Pauli_Y.Run(sim, q).Result)
        | "Z" -> Qubit_Val (Pauli_Z.Run(sim, q).Result)
        | _ -> invalidArg "code" "no, it can't, it's surreal"
    | _ -> too_many_args_err 1 arg

// =================== Quantum Part Ends ======================

// gives off a binary vector space of rank n
// n -> {0, 1}ⁿ
let private bin_vec_space rank =
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

let call_by_value sim interp_w_env name args =
    let values = List.map interp_w_env args
    match name with
    | "new" -> new_qubit values sim
    | "Qubits" -> new_qubits values sim
    | "H" | "X" | "Y" | "Z" -> basic_gate name values sim
    | _ -> not_implemented_err ()
    
let call_by_name sim interp_w_env name args =
    not_implemented_err ()
