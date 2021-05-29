namespace QAL.Utils

open QAL.Definitions.TypeDef
open Error

module Misc =

    /// <summary>
    /// converts array (any sequence) to F# list
    /// </summary>
    /// <param name="arr">the array</param>
    let arr_2_lst arr =
        [for e in arr do yield e]

    /// states if two values are equal. Only supports integer and complex for now
    let value_equal v1 v2 =
        match v1, v2 with
        | Integer_Val i1, Integer_Val i2 -> i1 = i2
        | Complex_Val(m1, a1), Complex_Val(m2, a2) -> m1 = m2 && a1 = a2
        | _ -> not_implemented_err ()

    /// <summary>
    /// asserts if <c>value</c> contains qubits. For example, it can be a tuple containing qubits
    /// </summary>
    let rec is_quantum_data value =
        match value with
        | Unit_Val 
        | String_Val _ 
        | Complex_Val _ 
        | Integer_Val _ 
        | Function_Red _ 
        | Function_Std _
        | Array_Val _ -> false
        | Qubit_Val _
        | System_Val _ -> true
        | Tuple_Val t -> List.exists is_quantum_data t
