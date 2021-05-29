namespace QAL.Library

open Quantum
open Arithmetic
open Constants
open CollectionOps
open IO
open Misc
open QAL.Utils.Error
open QAL.Definitions.TypeDef
open Microsoft.Quantum.Simulation.Simulators

module Locator =

    let private find_variable name =
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

    /// <summary>
    /// try to find a variable/standard libray function by the given name
    /// </summary>
    /// <param name="sim">the reference to quantum simulator</param>
    /// <param name="interp">the interp function with environment</param>
    /// <param name="name">name of the target</param>
    /// <returns><c>None</c> if it doesn't exist. Otherwise returns something</returns>
    let public find_std sim (interp: 'a list -> QuantumSimulator -> Expr -> Value) name =
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
            | "map" -> Some (Function_Std(name, (map (interp [] sim))))
            | "fold" -> Some (Function_Std(name, (fold (interp [] sim))))
            // qubit operations
            | "new" -> Some (Function_Std(name, (new_qubit sim)))
            | "System" -> Some (Function_Std(name, (new_qubits sim)))
            | "measure" -> Some (Function_Std(name, (measure sim)))
            | "H" | "X" | "Y" | "Z" | "CNOT" -> Some (Function_Std(name, (basic_gate sim name)))
            | _ -> None
