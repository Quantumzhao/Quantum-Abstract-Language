namespace QAL.Library

open QAL.Definitions.TypeDef
open QAL.Utils.Error

module CollectionOps =

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
    let split args =
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
    let index args =
        match args with
        | Integer_Val i :: Array_Val a :: [] -> a.Item i
        | Integer_Val i :: System_Val s :: [] -> 
            not_implemented_err ()
        | _ -> too_many_args_err 2 args
    
    /// <summary> the good ol' <c>map</c> function </summary>
    /// <param name="interp_w_sim">the interp function that
    /// has been partially applied with environment and quantum simulator</param>
    /// <param name="args">
    /// The signiture is: 
    /// - either <c>('a -> 'b) -> Array of 'a -> Array of 'b </c>
    /// - or <c>(Qubit -> 'a) -> System -> Array of 'a</c>
    /// - or <c>(Qubit -> Qubit) -> System -> System</c>
    /// </param>
    let map interp_w_sim (args: Value list) =
        let to_expr_list list =
            match list with
            | System_Val s -> List.map Literal s
            | Array_Val a -> List.map Literal a
            | _ -> invalidArg "list" "not a collection"
        match args with
        | func :: collec :: [] ->
            let eval'ed_collec = to_expr_list collec
            let apply_2_ele e = Apply(Literal func, [e])
            let final_collec = List.map (apply_2_ele >> interp_w_sim) eval'ed_collec
            Array_Val final_collec
        | _ -> not_implemented_err ()
    
    /// <summary> the good ol' left <c>fold</c> function </summary>
    /// <param name="interp_w_sim">the interp function that
    /// has been partially applied with environment and quantum simulator</param>
    /// <param name="args">
    /// The signiture is: 
    /// - either <c> accumulator: ('a -> 'x -> 'a) -> initial: 'a -> Array of 'x -> Array of 'a </c>
    /// - or <c>('a -> Qubit -> 'a) -> Array of 'a -> System -> Array of 'a</c>
    /// </param>
    let fold (interp_w_sim: Expr -> Value) (args: Value list) =
        let to_expr_list list =
            match list with
            | System_Val s -> List.map Literal s
            | Array_Val a -> List.map Literal a
            | _ -> invalidArg "list" "not a collection"
        match args with
        | func :: accumulator :: collec :: [] ->
            let eval'ed_collec = to_expr_list collec
            let apply_2_ele acc e = Apply(Literal func, [Literal acc; e])
            List.fold (fun acc x -> interp_w_sim (apply_2_ele acc x)) accumulator eval'ed_collec
        | _ -> not_implemented_err ()
    
    /// <summary> 
    /// first borrows some qubits described by the discriminator from a composite system, 
    /// do some operations and then put them back as well as returning the results
    /// </summary>
    /// <param name="interp_w_sim">the interp function that
    /// has been partially applied with environment and quantum simulator</param>
    /// <param name="args">
    /// The signiture is: 
    /// - <c>discriminator -> operation -> system -> result * system</c>
    /// </param>
    let borrow interp_w_sim args =
    
        not_implemented_err ()
