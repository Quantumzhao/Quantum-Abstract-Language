module Interpreter

open TypeDef
open Helper
open StandardLibrary
open Microsoft.Quantum.Simulation.Simulators
open Microsoft.Quantum.Simulation.Core

// TODO: qubit support
let rec find_variable env name =
    match env with
    | (entry_name, value) :: _ when entry_name = name -> Some value
    | _ :: rest -> find_variable rest name
    | [] -> None

/// <summary>
/// interprets the expression under the environment
/// </summary>
/// <param name="env">environment</param>
/// <param name="qs">qubits pool</param>
/// <param name="exp">expression</param>
let rec interp env qs exp = 
    match exp with
    // interp values
    | Integer i -> Integer_Val i
    | Complex(m, a) -> Complex_Val(m, a)
    | Qubit q -> Qubit_Val q
    // interp collections
    | Array exps -> interp_array env qs exps
    | System qexps -> interp_system env qs qexps
    | Tuple exps -> interp_tuple env qs exps
    // interp references
    | Variable v -> 
        match find_variable env v with
        | Some value -> value
        | None -> 
            match find_any v with
            | Some value -> value
            | None -> failwith $"{v} is not defined"
    // interp syntax structure
    | Apply(func, args) -> interp_apply env qs func args
    | Let_Fun(name, ps, body, in_expr) -> interp_let_fun env qs name ps body in_expr
    | Let_Var(name, binding, in_expr) -> interp_let_var env qs name binding in_expr
    | Match(cond, cases) -> interp_match env qs cond cases
    | Unit -> Unit_Val
    | _ -> not_implemented_err ()

// TODO: qubit support
and interp_array env qs exps =
    let result_vector = 
        List.map (interp env qs) exps
    in
    Array_Val result_vector

// TODO: qubit support
and interp_system env qs qexps =
    let result_vector =
        List.map (interp env qs) qexps
    in
    System_Val result_vector

// TODO: qubit support
and interp_tuple env qs exps =
    let result_vector =
        List.map (interp env qs) exps
    in
    Tuple_Val result_vector

// TODO: qubit support
and interp_let_fun env qs name ps body in_expr =
    // put all info into the reduced function without changing anything
    // since here it follows call by name
    let fun_red = Function_Red(name, ps, body)
    let new_env = (name, fun_red) :: env
    interp new_env qs in_expr

// TODO: qubit support
and interp_let_var env qs name exp in_expr =
    let res = interp env qs exp
    let new_env = (name, res) :: env
    interp new_env qs in_expr

// not sure if it is compatible with qubits
and interp_apply env qs func args =
    // whatever what the expression might be, evaluate it first
    // it may actually be a variable or expression, doesn't matter
    let id = interp env qs func
    match id with
    | Function_Red(name, ps, body) when ps.Length = args.Length -> 
        // first evaluate all the arguments
        let eval'ed_args = List.map (interp env qs) args
        // then pair them with parameters
        let param_arg_pair = List.zip ps eval'ed_args
        // pack into a set of environment
        // sorta hack, should replace it if there's any better way
        let new_env = List.append param_arg_pair param_arg_pair
        interp new_env qs body
    | Function_Red(_, ps, _) -> 
        failwith $"argument number mismatch: expect {ps.Length}, actual {args.Length}"
    | other when args.Length <> 0 -> 
        failwith $"{other} is not a function, hence cannot apply {args}"
    // apply "apply" to values. wierd, but acceptable
    | other -> other

// TODO: qubit support
and interp_match env qs cond cases =
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
                let new_env = (p, cond) :: env
                // evaluate the branch
                interp new_env qs expr
            | WildCard -> interp env qs expr
            | Int_Lit i when cond = Integer_Val i -> interp env qs expr
            | Comp_Lit m when cond = Complex_Val(m, 0m) -> interp env qs expr
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
                    | Placeholder p -> (p, value) :: accumulator
                    | _ -> accumulator
                // if this tuple of values indeed satidfies the pattern
                if List.fold (fun acc x -> acc && (match_rule x)) true p_v_pair then
                    // put all qualified binded variables (from the pattern) into a new list
                    let matched_pairs = List.foldBack fill_in_pattern p_v_pair []
                    // add them to th environment
                    let new_env = List.append matched_pairs env
                    interp new_env qs expr
                else
                    // otherwise jump to next
                    interp_match_rec env cond rest_c
        | _, _ -> 
         failwith $"{cond} currently is not supoorted in pattern matching"
    // ----| starts here |----
    interp_match_rec env (interp env qs cond) cases
