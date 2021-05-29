namespace QAL.Library

open QAL.Definitions.TypeDef
open QAL.Utils.Error

module Misc =
    
    /// for now, it only supprts numerical data
    let equals args =
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
    
    /// gives off a binary vector space of rank n.
    /// n -> {0, 1}ⁿ
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
    