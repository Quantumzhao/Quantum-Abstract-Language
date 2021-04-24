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

    
/// <summary>
/// calls a standard library function
/// </summary>
/// <param name="interp_w_env">an interpret function that has been partially applied with environment</param>
/// <param name="func">the already-partially-applied standard library function</param>
/// <param name="args">arguments of the function</param>
let public call_std interp_w_env func args =
    let values = List.map interp_w_env args
    func values

/// <summary>
/// calls a standard library function
/// </summary>
/// <param name="name">name of the function</param>
/// <param name="interp_w_env">an interpret function that has been partially applied with environment</param>
/// <param name="func">the already-partially-applied standard library function</param>
/// <param name="args">arguments of the function</param>
let public call_std_by_name (interp_w_env: Expr -> Value) name func args =
    // call by name
    match name with
    | "map" -> func interp_w_env args
    | _ -> not_implemented_err ()

let complex_to_decimal c =
    match c with
    | Complex_Val(m, a) -> 
        // if c is positive real
        if a % 2m = 0m then
            Some m
        // if c is negative real
        elif a % 2m = 1m || a % 2m = -1m then
            Some -m
        // if c is complex
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

let complex_constructor args =
    // convert the modulus and argument to decimal, regardless whether it's an integer or decimal
    match List.map (to_complex >> complex_to_decimal) args with
    | Some m :: Some a :: []  -> Complex_Val(m, a)
    | _ -> invalidArg "modulus and argument" "either one of them is a complex number"

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
    // general case
    | Complex_Val(m1, a1) :: Integer_Val i :: [] -> 
        add [(Complex_Val(m1, a1)); (to_complex (Integer_Val i))]
    | Integer_Val i :: Complex_Val(m2, a2) :: [] -> 
    // symmetric general case
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

/// for now, it only supprts numerical data
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

/// allocate a new composite system
let private new_qubits (sim: QuantumSimulator) arg =
    match arg with
    | Integer_Val n :: [] -> 
        // a QArray of Q# qubits
        let q_array = sim.QubitManager.Allocate(int64 n)
        // an F# list of my qubits
        let proc'ed_arr = List.map Qubit_Val (arr_2_lst q_array)
        // a system of my qubits
        System_Val proc'ed_arr
    | _ -> too_many_args_err 1 arg

/// allocate a single new qubit (possibly an ancilla)
let private new_qubit (sim: QuantumSimulator) args =
    match args with
    | Integer_Val option :: [] -> 
        let qubit = sim.QubitManager.Allocate()
        match option with
        | 0 -> Qubit_Val qubit
        | 1 -> Qubit_Val (XGate.Run(sim, qubit).Result)
        | 2 -> Qubit_Val (HGate.Run(sim, qubit).Result)
        | 3 -> 
            let q1 = XGate.Run(sim, qubit).Result
            Qubit_Val (HGate.Run(sim, q1).Result)
        | _ -> invalidArg "option" "no, it can't, it's surreal"
    | _ -> too_many_args_err 1 args

/// measures the qubit, and returns the result together with a new state of the qubit
let private measure_n_reset sim args =
    match args with
    | Qubit_Val q :: [] -> 
        let struct(res, qubit) = Measure.Run(sim, q).Result
        if res = Result.One then Tuple_Val[Integer_Val 1; Qubit_Val qubit]
        else Tuple_Val[Integer_Val 0; Qubit_Val qubit]
    | _ -> too_many_args_err 1 args

/// the classical measure operation. Can only return the classical result
let private measure sim args =
    match measure_n_reset sim args with
    | Tuple_Val(res :: _) -> res
    | other -> syntax_err Tuple_Val other

let private basic_gate sim code arg =
    match arg with
    // 1 qubit gates
    | Qubit_Val q :: [] -> 
        match code with
        | "H" -> Qubit_Val (HGate.Run(sim, q).Result)
        | "X" -> Qubit_Val (XGate.Run(sim, q).Result)
        | "Y" -> Qubit_Val (YGate.Run(sim, q).Result)
        | "Z" -> Qubit_Val (ZGate.Run(sim, q).Result)
        | _ -> invalidArg "code" "no, it can't, it's surreal"
    // 2 qubit gates
    | Qubit_Val ctl :: Qubit_Val tgt :: [] -> 
        match code with
        | "CNOT" -> 
            let struct(ctl', tgt') = ControlledXGate.Run(sim, ctl, tgt).Result
            Tuple_Val [Qubit_Val ctl'; Qubit_Val tgt']
        | _ -> invalidArg "code" "no, it can't, it's surreal"
    | _ -> too_many_args_err 1 arg

// =================== Quantum Part Ends ======================

/// <summary>
/// asserts if <c>value</c> contains qubits. For example, it can be a tuple containing qubits
/// </summary>
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

/// <summary>raise the base to a specified power. Same for functions</summary>
/// <remarks>the power cannot be negative</remarks>
let private pow interp_w_env args =
    match args with
    | Integer_Val basei :: Integer_Val power :: [] -> 
        let res = DecimalEx.Pow(decimal basei, decimal power)
        Integer_Val (int res)
    | Complex_Val(m, a) :: Integer_Val i :: [] ->
        let m' = DecimalEx.Pow(m, decimal i)
        let a' = DecimalEx.Pow(a, decimal i)
        Complex_Val(m', a')
    (*| Function_Red(name, env, ps, body) :: Integer_Val power :: [] ->
        let rec rec_pow (op : 'a -> 'a) pow : ('a -> 'a) =
            if pow = 0 then
                fun x -> x
            else
                fun x -> op ((rec_pow op (pow - 1)) x)
        if power < 0 then
            not_implemented_err ()
        else
            not_implemented_err ()
    | Function_Std(name, body) :: Integer_Val power :: [] -> 
        let rec rec_pow (op : 'a -> 'a) pow : ('a -> 'a) =
            if pow = 0 then
                fun x -> x
            else
                fun x -> op ((rec_pow op (pow - 1)) x)
        if power < 0 then
            not_implemented_err ()
        else
            (fun arg -> )*)
    | _ -> too_many_args_err 2 args

/// gives off a binary vector space of rank n.
/// n -> {0, 1}ⁿ
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

/// <summary>returns a range by the given start int, end int and step. (inclusive)</summary>
/// <remarks>
/// The possible overloads: 
/// - <c>Integer -> Integer -> Integer -> Array</c>: start, end, step
/// - <c>Integer -> Integer -> Array</c>: start, end. Assume step to be 1
/// - <c>Integer -> Array</c>: end. Assume starts from 0 and step 1
/// </remarks>
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

/// <returns>A tuple containing the first element and rest</returns>
/// <remarks>
/// Overloads: 
/// 1. <c>Array -> Tuple(Value, Array)</c>
/// 2. <c>System -> Tuple(Qubit, System)</c>
/// </remarks>
let head_n_tail args =
    match args with
    | Array_Val (head :: tail) :: [] -> Tuple_Val [head; Array_Val tail]
    | System_Val (head :: tail) :: [] -> Tuple_Val [head; Array_Val tail]
    | _ -> too_many_args_err 1 args

/// <summary>returns the last element of an array or composite system</summary>
/// <remarks>
/// Overloads: 
/// 1. <c>Integer -> Array -> Data</c>
/// 2. <c>Integer -> System -> Qubit</c>
/// </remarks>
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

/// <summary>splits a collection into 2 by the given index</summary>
/// <remarks>
/// Overloads: 
/// 1. <c>Integer -> Array -> Tuple(Array, Array)</c>
/// 2. <c>Integer -> System -> Tuple(System, System)</c>
/// </remarks>
let private split args =
    match args with
    | Integer_Val i :: Array_Val a :: [] -> 
        let t1, t2 = List.splitAt i a
        Tuple_Val [Array_Val t1; Array_Val t2]
    | Integer_Val i :: System_Val s :: [] -> 
        not_implemented_err ()
    | _ -> too_many_args_err 2 args

/// <summary>
/// returns the element indexed by the index. 
/// for composite system, it also means the old collection is completely discarded
/// </summary>
/// <remarks>
/// Overloads: 
/// 1. <c>Integer -> Array -> Data</c>
/// 2. <c>Integer -> System -> Qubit</c>
/// </remarks>
let private index args =
    match args with
    | Integer_Val i :: Array_Val a :: [] -> a.Item i
    | Integer_Val i :: System_Val s :: [] -> 
        not_implemented_err ()
    | _ -> too_many_args_err 2 args

let private map sim interp (args: Value list) =
    let to_expr_list list =
        match list with
        | System_Val s -> List.map Literal s
        | Array_Val a -> List.map Literal a
        | _ -> invalidArg "list" "not a collection"
    match args with
    | func :: collec :: [] ->
        let eval'ed_collec = to_expr_list collec
        let apply_2_ele e = Apply(Literal func, [e])
        let final_collec = List.map (apply_2_ele >> (interp [] sim)) eval'ed_collec
        Array_Val final_collec
    | _ -> not_implemented_err ()

/// <summary>
/// try to find a variable/standard libray function by the given name
/// </summary>
/// <param name="sim">the reference to quantum simulator</param>
/// <param name="interp">the interp function with environment</param>
/// <param name="name">name of the target</param>
/// <returns><c>None</c> if it doesn't exist. Otherwise returns something</returns>
let public find sim (interp: 'a list -> QuantumSimulator -> Expr -> Value) name =
    match find_variable name with
    | Some v -> Some v
    | None -> 
        match name with
        // basic operations
        | "add" -> Some (Function_Std(name, add))
        | "print" -> Some (Function_Std(name, print))
        | "bin_vec_space" -> Some (Function_Std(name, bin_vec_space))
        | "Complex" -> Some (Function_Std(name, complex_constructor))
        // collection operations
        | "head_n_tail" -> Some (Function_Std(name, head_n_tail))
        | "last" -> Some (Function_Std(name, last))
        | "range" -> Some (Function_Std(name, range))
        | "map" -> Some (Function_Std(name, (map sim interp)))
        // qubit operations
        | "new" -> Some (Function_Std(name, (new_qubit sim)))
        | "Qubits" -> Some (Function_Std(name, (new_qubits sim)))
        | "measure" -> Some (Function_Std(name, (measure sim)))
        | "H" | "X" | "Y" | "Z" | "CNOT" -> Some (Function_Std(name, (basic_gate sim name)))
        | _ -> None
