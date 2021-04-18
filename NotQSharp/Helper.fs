module Helper

open TypeDef


let not_implemented_err () = 
    failwith "not implemented"

let syntax_err expect actual =
    failwith $"syntax error: expect {expect}, actual {actual}"

let no_such_element_err arg collection =
    failwith $"cannot find {arg} in {collection}"

/// <summary>
/// converts array (any sequence) to F# list
/// </summary>
/// <param name="arr">the array</param>
let arr_2_lst arr =
    [for e in arr do yield e]

/// given a list of tokens, outputs a complete list(string) of the tokens
let pretty_print tokens = 
    List.fold (fun acc t -> acc + $"[{t.ToString()}]" + " ") "" tokens

// draws an AST
let rec pretty_draw (ast: Expr) =
    let add_bracket str = $"[{str}]"
    let multi_node name content = add_bracket (name.ToString() + ": " + content)
    match ast with
    | Integer _ | Complex _ | String _ | Unit | Variable _ | Qubit _ -> add_bracket ast
    | Tuple t -> multi_node "Tuple" (pretty_print t)
    | Array a -> multi_node "Array" (pretty_print a)
    | System s -> multi_node "System" (pretty_print s)
    | Apply(func, args) -> 
        let drawn_func = pretty_draw func
        let drawn_args_list = List.map pretty_draw args
        let flattened_args = List.fold (fun acc x -> acc + add_bracket x + " ") "" drawn_args_list
        multi_node "Apply" (drawn_func + " " + flattened_args)
    | Let_Var(name, body, expr) -> 
        let drawn_body = pretty_draw body
        let draw_expr = pretty_draw expr
        multi_node "Let_Var" (name + " " + drawn_body + draw_expr)
    | Let_Fun(name, ps, body, expr) -> 
        let drawn_params = pretty_print ps
        let drawn_body = pretty_draw body
        let drawn_expr = pretty_draw expr
        multi_node "Let_Fun" (name + " " + drawn_params + drawn_body + drawn_expr)
    | Match(cond, cases) -> 
        let drawn_cond = pretty_draw cond
        let draw_single_case (patterns, expr) = 
            patterns.ToString() + ": " + (pretty_draw expr)
        let drawn_cases = pretty_print (List.map draw_single_case cases)
        multi_node "Match" (drawn_cond + drawn_cases)
    | _ -> invalidArg "ast" "it's not possible!"

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
    | Function_Std _ -> false
    | Array_Val _ -> false
    | Qubit_Val _ -> true
    | System_Val _ -> true
    | Tuple_Val t -> List.exists is_quantum_data t

/// <summary>
/// find whether there is a tuple (string * value) in env with given binding as the string. If exist, return the corresponding value of the given binding string and return the env without that tuple. If not, return (Unit_Val, env) with env unchanged.
/// </summary>
let rec private find_binding binding env = 
    List.fold (fun acc elem -> let (str, value) = elem in 
                                    let (new_value, lst) = acc in
                                        match new_value with
                                        | Unit_Val -> 
                                            if (str.Equals(binding)) then 
                                                (value, lst)
                                            else
                                                (new_value,(elem :: lst))  
                                        | _ -> (new_value,(elem :: lst))) (Unit_Val, []) env 
    // match env with
    // |h :: t -> let (str, value) = h in if (str.Equals(binding)) then (value, true) else (find_binding t binding)
    // |[] -> (Unit_Val, false)

/// <summary>
/// If a binding exist in env and the corresponding value is a quantum data type, then delete this binding tuple in env and return the corresponding value. Otherwise, return (Unit_Val, env) with env unchanged.
/// </summary>
let eliminate_q binding env =
    let (result_value, new_env) = (find_binding binding env) in
        if (is_quantum_data result_value) then (result_value, new_env) else (Unit_Val, env)

/// <summary>
/// Add a bind_tuple into environment, since this is too easy, there might be an misunderstanding. 
/// </summary>            
let reinsert_q bind_tuple env =
    (bind_tuple :: env)



