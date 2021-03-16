module Interpreter

open TypeDef
open Helper

let find_variable env name =
    not_implemented_err ()

/// <summary>
/// interprets the expression under the environment
/// </summary>
/// <param name="env">environment</param>
/// <param name="exp">expression</param>
let rec interp env exp = 
    match exp with
    // interp values
    | Integer i -> Integer_Val i
    | Complex(m, a) -> Complex_Val(m, a)
    | Qubit q -> Qubit_Val q
    // interp collections
    | Array exps -> interp_array env exps
    | System qexps -> interp_system env qexps
    | Tuple exps -> interp_tuple env exps
    // interp references
    | Variable v -> find_variable env v
    // interp syntax structure
    | Apply _ -> interp_apply env exp
    | Let_Fun _ -> interp_let_fun env exp
    | Let_Var _ -> interp_let_var env exp
    | Match _ -> interp_match env exp
    | Unit -> Unit_Val
    | _ -> not_implemented_err ()

and interp_array env exps =
    let result_vector = 
        List.map (interp env) exps
    in
    Array_Val result_vector

and interp_system env qexps =
    let result_vector =
        List.map (interp env) qexps
    in
    System_Val result_vector

and interp_tuple env exps =
    let result_vector =
        List.map (interp env) exps
    in
    Tuple_Val result_vector

and interp_let_fun env exp =
    not_implemented_err ()

and interp_let_var env exp =
    not_implemented_err ()

and interp_apply env exp =
    not_implemented_err ()

and interp_function env exp =
    not_implemented_err ()

and interp_match env exp =
    not_implemented_err ()
