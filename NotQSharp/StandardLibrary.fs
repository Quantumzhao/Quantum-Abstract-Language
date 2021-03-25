module StandardLibrary

open Microsoft.Quantum.Simulation.Core
open System
open Helper
open DecimalMath
open QuantumLib
open TypeDef
open DecimalMath
open Microsoft.Quantum.Canon
open Microsoft.Quantum.Simulation.Simulators

// TODO: 
// map: the familiar map. (a' -> b') -> a' list -> b' list
// fold: the familiar fold. (a' -> b' -> a') -> a' -> b' list -> a'
// borrow: borrows a set of qubits, do some operations, and then return the qubits back to the set
//         (System -> a') -> System -> System * a'

let private too_many_args_err (expect: int) (actual_arg_list: Value list) =
    syntax_err $"{expect} args" $"{actual_arg_list.Length} args"

let private pi = Complex_Val(DecimalEx.Pi, 0m)

let private e = Complex_Val(DecimalEx.E, 0m)

let private i = Complex_Val(1m, DecimalEx.Pi)

let public find_variable name =
    match name with
    | "pi" -> Some pi
    | "e" -> Some e
    | "i" -> Some i
    | _ -> None

let complex_to_decimal c =
    match c with
    | Complex_Val(m, a) -> 
        if a % 2m = 0m then
            Some m
        elif a % 2m = 1m || a % 2m = -1m then
            Some -m
        else
            None
    | _ -> invalidArg "complex" "expect complex"

let to_int c =
    match c with
    | Complex_Val(m, a) -> 
        match complex_to_decimal (Complex_Val(m, a)) with
        | Some d -> int (round d)
        | None -> 
            invalidArg "complex" "imaginary part is not 0: ambiguous rounding"
    | Integer_Val i -> i
    | other -> invalidArg "complex" $"cannot cast {other} to integer"

let to_complex num =
    match num with
    | Integer_Val i when i >= 0 -> Complex_Val(decimal i, 0m)
    | Integer_Val i when i < 0 -> Complex_Val(decimal i, 1m)
    | Complex_Val(m, a) -> Complex_Val(m, a)
    | other -> invalidArg "number" $"cannot cast {other} to integer"

let rec private add args =
    match args with
    // integer addition
    | Integer_Val i1 :: Integer_Val i2 :: [] -> Integer_Val(i1 + i2)
    | Complex_Val(m1, a1) :: Complex_Val(m2, a2) :: [] -> 
        let s1, s2 = complex_to_decimal (Complex_Val(m1, a1)), complex_to_decimal (Complex_Val(m2, a2))
        match s1, s2 with
        // real addition
        | Some d1, Some d2 -> 
            if m1 + m2 < 0m then
                Complex_Val (-d1 - d2, 1m)
            else
                Complex_Val (d1 + d2, 0m)
        // complex addition
        // why would anyone want to do this ?
        | _, _ -> 
            let argand_to_cartesian m a =
                m * DecimalEx.Cos (a * DecimalEx.Pi), m * DecimalEx.Sin (a * DecimalEx.Pi)
            let r1, i1 = argand_to_cartesian m1 a1
            let r2, i2 = argand_to_cartesian m2 a2
            let r = r1 + r2
            let i = i1 + i2
            let m = DecimalEx.Sqrt (r * r + i * i)
            let a = DecimalEx.ATan (i / r) / DecimalEx.Pi
            Complex_Val(m, a)
    | Complex_Val(m1, a1) :: Integer_Val i :: [] -> 
        add [(Complex_Val(m1, a1)); (to_complex (Integer_Val i))]
    | Integer_Val i :: Complex_Val(m2, a2) :: [] -> 
        add [(to_complex (Integer_Val i)); (Complex_Val(m2, a2))]
    | other1 :: other2 :: [] -> 
        invalidArg "expression 1 or expression 2" $"cannot cast either {other1} or {other2} to integer"
    | _ -> too_many_args_err 2 args

let rec private print args =
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
    | Tuple_Val items :: tl -> 
        not_implemented_err ()
    | _ -> too_many_args_err 1 args

let private equals args =
    match args with
    | Integer_Val i1 :: Integer_Val i2 :: [] -> 
        if i1 = i2 then Integer_Val 1
        else Integer_Val 0
    | Complex_Val(m1, a1) :: Complex_Val(m2, a2) :: [] ->
        if abs a1 % 2m = abs a2 % 2m && m1 = m2 then
            Integer_Val 1
        else 
            Integer_Val 0
    | _ -> too_many_args_err 2 args

// =============== Quantum Part ==================
// require refactor in the future

let private new_qubits arg (sim: QuantumSimulator) =
    match arg with
    | Integer_Val n :: [] -> 
        // a QArray of Q# qubits
        let q_array = sim.QubitManager.Allocate(int64 n)
        // an F# list of my qubits
        let proc'ed_arr = List.map Qubit_Val (arr_2_lst q_array)
        // a system of my qubits
        System_Val proc'ed_arr
    | _ -> too_many_args_err 1 arg

let private new_qubit args (sim: QuantumSimulator) =
    match args with
    | Integer_Val option :: [] -> 
        let qubit = sim.QubitManager.Allocate()
        match option with
        | 0 -> Qubit_Val qubit
        | 1 -> Qubit_Val (Pauli_X.Run(sim, qubit).Result)
        | 2 -> Qubit_Val (Hadamard.Run(sim, qubit).Result)
        | 3 -> 
            let q1 = Pauli_X.Run(sim, qubit).Result
            Qubit_Val (Hadamard.Run(sim, q1).Result)
        | _ -> invalidArg "option" "no, it can't, it's surreal"
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
    // 1 qubit gates
    | Qubit_Val q :: [] -> 
        match code with
        | "H" -> Qubit_Val (Hadamard.Run(sim, q).Result)
        | "X" -> Qubit_Val (Pauli_X.Run(sim, q).Result)
        | "Y" -> Qubit_Val (Pauli_Y.Run(sim, q).Result)
        | "Z" -> Qubit_Val (Pauli_Z.Run(sim, q).Result)
        | _ -> invalidArg "code" "no, it can't, it's surreal"
    // 2 qubit gates
    | Qubit_Val ctl :: Qubit_Val tgt :: [] -> 
        match code with
        | "CNOT" -> 
            let struct(ctl', tgt') = Controlled_Pauli_X.Run(sim, ctl, tgt).Result
            Tuple_Val [Qubit_Val ctl'; Qubit_Val tgt']
        | _ -> invalidArg "code" "no, it can't, it's surreal"
    | _ -> too_many_args_err 1 arg

// =================== Quantum Part Ends ======================

let rec private is_quantum_data value =
    match value with
    | Unit_Val 
    | String_Val _ 
    | Complex_Val _ 
    | Integer_Val _ 
    | Function_Red _ 
    | Function_Std _ -> false
    | Array_Val _ -> false
    | Qubit_Val _ -> true
    | System_Val _ -> true
    | Tuple_Val t -> List.exists is_quantum_data t

let rec private pow args =
    match args with
    | Integer_Val basei :: Integer_Val power :: [] -> 
        DecimalEx.Pow(decimal basei, decimal power)
    | Function_Red(name, ps, body) :: Integer_Val power :: [] ->
        not_implemented_err ()
    | _ -> too_many_args_err 2 args

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
    | Integer_Val i :: [] -> flist_to_my_list (bin_vec_space_rec i)
    | _ -> invalidArg "rank" "rank must be an integer"

// returns a range by the given start int, end int and step
let range args =
    let rec generate_range start end' step finished =
        if start <= end' && end' <= (start + step) then
            start :: finished
        else
            start :: (generate_range (start + step) end' step finished)
    match args with
    | Integer_Val start :: Integer_Val end' :: Integer_Val step :: [] -> 
        let list = generate_range start end' step []
        Array_Val (List.map Integer_Val list)
    | Integer_Val start :: Integer_Val end' :: [] -> 
        let list = generate_range start end' 1 []
        Array_Val (List.map Integer_Val list)
    | Integer_Val end' :: [] -> 
        let list = generate_range 0 end' 1 []
        Array_Val (List.map Integer_Val list)
    | _ -> too_many_args_err 3 args

let head args =
    match args with
    | Array_Val (head :: _) :: [] -> head
    | System_Val (head :: _) :: [] -> head
    | _ -> too_many_args_err 1 args

let tail args =
    match args with
    | Array_Val a :: [] -> Array_Val a.Tail
    | System_Val s :: [] -> System_Val s.Tail
    | _ -> too_many_args_err 1 args

let last args =
    match args with
    | Array_Val a :: [] -> 
        let rec get_last arr =
            match arr with
            | [] -> invalidArg "array" "empty array"
            | last :: [] -> last
            | _ :: tl -> get_last tl
        get_last a
    | System_Val s :: [] -> not_implemented_err ()
    | _ -> too_many_args_err 1 args

// splits a collection into 2 by the given index
let private split args =
    match args with
    | Integer_Val i :: Array_Val a :: [] -> 
        let t1, t2 = List.splitAt i a
        Tuple_Val [Array_Val t1; Array_Val t2]
    | Integer_Val i :: System_Val s :: [] -> 
        not_implemented_err ()
    | _ -> too_many_args_err 2 args

// returns the element indexed by the index
// for composite system, it also means the old collection is completely discarded
let private index args =
    match args with
    | Integer_Val i :: Array_Val a :: [] -> a.Item i
    | Integer_Val i :: System_Val s :: [] -> 
        not_implemented_err ()
    | _ -> too_many_args_err 2 args

let public call sim (interp_w_env: Expr -> Value) name args =
    // call by name
    match name with
    | _ -> 
        // call by value
        let values = List.map interp_w_env args
        match name with
        // basic operations
        | "add" -> add values
        | "print" -> print values
        | "bin_vec_space" -> bin_vec_space values
        // collection operations
        | "head" -> head values
        | "tail" -> tail values
        | "last" -> last values
        | "range" -> range values
        // qubit operations
        | "new" -> new_qubit values sim
        | "Qubits" -> new_qubits values sim
        | "measure" -> measure values sim
        | "H" | "X" | "Y" | "Z" | "CNOT" -> basic_gate name values sim
        | _ -> not_implemented_err ()
