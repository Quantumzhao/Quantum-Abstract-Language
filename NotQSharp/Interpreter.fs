module Interpreter

open TypeDef

let not_implemented_err = 
    "not implemented"

let find_variable env name =
    failwith not_implemented_err

/// <summary>
/// interprets the expression under the environment
/// </summary>
/// <param name="env">environment</param>
/// <param name="exp">expression</param>
let rec interp env exp = 
    match exp with
    | Integer i -> Integer_Val i
    | Complex(m, a) -> Complex_Val(m, a)
    | String s -> String_Val s
    | Unit -> Unit_Val
    | Array exps -> interp_array env exps
    | Variable v -> find_variable env v
    | Apply _ -> interp_apply env exp
    | StdApply _ -> interp_std_apply env exp
    | Let _ -> interp_let env exp
    | Function _ -> interp_function env exp
    | Qubit q -> Qubit_Val q
    | _ -> failwith "type error"

and interp_let env exp =
    failwith not_implemented_err

and interp_apply env exp =
    failwith not_implemented_err

and interp_std_apply env exp =
    failwith not_implemented_err

and interp_function env exp =
    failwith not_implemented_err

and interp_array env exps =
    let result_vector = 
        List.map (interp env) exps
    in
    Array_Val result_vector
