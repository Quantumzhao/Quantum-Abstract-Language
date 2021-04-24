module Interpreter

open TypeDef
open Helper
open StandardLibrary
open Microsoft.Quantum.Simulation.Simulators
open Microsoft.Quantum.Simulation.Core

// TODO: qubit support
/// <summary>
/// If a binding exist in env and the corresponding value is a quantum data type, then change the the flag in the binding to true and return the corresponding value. Otherwise, just return the corresponding value.
/// </summary>
let rec find_defined env name =
    match env with
    | (entry_name, value, flag) :: _ when entry_name = name -> 
        if !flag then failwith $"The quantum varible: '{entry_name}' can not be used again!" 
        elif is_quantum_data value then let _ = flag := true in Some value
        else Some value
    | first :: rest -> find_defined rest name
    | [] -> None

/// <summary>
/// interprets the expression under the environment
/// </summary>
/// <param name="env">environment</param>
/// <param name="sim">simulator</param>
/// <param name="exp">expression</param>
let rec interp (env: (string * Value * (bool ref)) list) sim exp = 
    match exp with
    // interp values
    | Integer i -> Integer_Val i
    | Complex(m, a) -> Complex_Val(m, a)
    | Qubit q -> Qubit_Val q
    | String s -> String_Val s
    // interp collections
    | Array exps -> interp_array env sim exps
    | System qexps -> interp_system env sim qexps
    | Tuple exps -> interp_tuple env sim exps
    // interp references
    | Variable v -> interp_variable env sim v
    // interp syntax structure
    | Apply(func, args) -> interp_apply env sim func args
    | Let_Fun(name, ps, body, in_expr) -> interp_let_fun env sim name ps body in_expr
    | Let_Var(name, binding, in_expr) -> interp_let_var env sim name binding in_expr
    | Match(cond, cases) -> interp_match env sim cond cases
    | Unit -> Unit_Val
    | _ -> failwith "it's not possible!"

and interp_variable env (sim: QuantumSimulator) v =
        // first try to find variable and function in environment
        match find_defined env v with
        | Some value -> value
        | None -> 
            // then try to find variables/functions defined in standard library
            match find sim interp v with
            | Some value -> value
            // if find nothing, then the variable is either not in scope at all 
            // or it is a qubit and its ownership has been transfered
            | None -> failwith $"{v} does not exist in the current space-time frame"

// TODO: qubit support
and interp_array env sim exps =
    let result_vector = 
        List.map (interp env sim) exps
    in
    Array_Val result_vector

// TODO: qubit support
and interp_system env sim qexps =
    let result_vector =
        List.map (interp env sim) qexps
    in
    System_Val result_vector

// TODO: qubit support
and interp_tuple env sim exps =
    let result_vector =
        List.map (interp env sim) exps
    in
    Tuple_Val result_vector

// TODO: qubit support
and interp_let_fun env sim name ps body in_expr =
    // put all info into the reduced function without changing anything
    // since here it follows call by name
    let fun_red = Function_Red(name, env, ps, body)
    let new_env = (name, fun_red, ref false) :: env
    interp new_env sim in_expr

// TODO: qubit support
and interp_let_var env sim name exp in_expr =
    let res = interp env sim exp
    let new_env = (name, res, ref false) :: env
    interp new_env sim in_expr

// not sure if it is compatible with qubits
and interp_apply env sim func args =
    // whatever what the expression might be, evaluate it first
    // it may actually be a variable or expression, doesn't matter
    let id = interp env sim func
    match id with
    | Function_Red(name, closure, ps, body) when ps.Length = args.Length -> 
        // first evaluate all the arguments
        let eval'ed_args = List.map (interp env sim) args
        // then pair them with parameters
        let param_arg_pair = List.map (fun (a, b) -> a,b,ref false) (List.zip ps eval'ed_args)
        let fun_n_pa_pair = (name, id, ref false) :: param_arg_pair
        // pack into a set of environment
        // sorta hack, should replace it if there's any better way
        let new_env = List.append fun_n_pa_pair closure
        interp new_env sim body
    | Function_Std(_, func) -> call_std (interp env sim) func args
    | Function_Red(_, _, ps, _) -> 
        failwith $"argument number mismatch: expect {ps.Length}, actual {args.Length}"
    | other when args.Length <> 0 -> 
        failwith $"{other} is not a function, hence cannot apply {args}"
    // apply "apply" to values. wierd, but acceptable
    | other -> other

// TODO: qubit support
and interp_match env sim cond cases =
    // cond: condition
    let rec interp_match_rec env cond cases =
        match cases, cond with
        // when no available cases left, and condition is a unit
        // wierd, but acceptable
        | [], Unit_Val -> Unit_Val
        // running out of cases, no match cases
        | [], _ -> failwith $"no match cases with {cond}"
        // when the pattern consists of only one placeholder/wildcard
        | (pattern :: [], expr) :: rest, _ -> 
            match pattern with
            | Placeholder p -> 
                // put the value of condition expression in the environment
                let new_env = (p, cond, ref false) :: env
                // evaluate the branch
                interp new_env sim expr
            | WildCard -> interp env sim expr
            | Int_Lit i when value_equal cond (Integer_Val i) -> interp env sim expr
            | Comp_Lit m when value_equal cond (Complex_Val(m, 0m)) -> interp env sim expr
            // if no match, jump to next
            | _ -> interp_match_rec env cond rest
        // when the pattern is a tuple and the condition returns a tuple as well
        | (patterns, expr) :: rest_c, Tuple_Val vs ->
            // asserts if the lengths match
            // we should probably accept cases when length mismatch
            if patterns.Length <> vs.Length then
                interp_match_rec env cond rest_c
            else
                // p_v_pair: the list of pairs, 
                // and each of them consists of a single placeholder/wildcard 
                // and an evaluated value from the condition tuple
                let p_v_pair = List.zip patterns vs
                // states if the value satifies the pattern described
                let match_rule (pattern, value) =
                    match pattern, value with
                    | Int_Lit i1, Integer_Val i2 -> i1 = i2
                    | Comp_Lit m1, Complex_Val(m2, a2) -> m1 = m2 && a2 = 0m
                    // by default, placeholder and wildcard are considered matchable
                    | _ -> true
                // assumes the given pattern and value is compatible
                // extracts the variable in the pattern (if applicable)
                // and put it into a new list
                let fill_in_pattern (pattern, value) accumulator = 
                    match pattern with
                    // meaningful binding
                    | Placeholder p -> (p, value, ref false) :: accumulator
                    | _ -> accumulator
                // if this tuple of values indeed satidfies the pattern
                if List.fold (fun acc x -> acc && (match_rule x)) true p_v_pair then
                    // put all qualified binded variables (from the pattern) into a new list
                    let matched_pairs = List.foldBack fill_in_pattern p_v_pair []
                    // add them to th environment
                    let new_env = List.append matched_pairs env
                    interp new_env sim expr
                else
                    // otherwise jump to next
                    interp_match_rec env cond rest_c
        | _, _ -> 
         failwith $"{cond} currently is not supoorted in pattern matching"
    // ----| starts here |----
    interp_match_rec env (interp env sim cond) cases
