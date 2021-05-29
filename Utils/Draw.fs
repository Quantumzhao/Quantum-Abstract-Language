namespace QAL.Utils

open QAL.Definitions.TypeDef

module Draw =

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

