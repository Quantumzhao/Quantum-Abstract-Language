module Helper

open TypeDef

let not_implemented_err () = 
    failwith "not implemented"

let syntax_err expect actual =
    failwith $"syntax error: expect {expect}, actual {actual}"

/// given a list of tokens, outputs a complete list(string) of the tokens
let pretty_print tokens = 
    List.fold (fun acc t -> acc + $"[{t.ToString()}]" + " ") "" tokens

// draws an AST
let rec pretty_draw (ast: Expr) =
    let add_bracket str = $"[{str}]"
    let multi_node name content = add_bracket (name.ToString() + ": " + content)
    match ast with
    | Integer _ | Complex _ | String _ | Unit | Variable _ | Qubit _ -> $"[{ast.ToString()}]"
    | Tuple t -> multi_node "Tuple" (pretty_print t)
    | Array a -> multi_node "Array" (pretty_print a)
    | System s -> multi_node "System" (pretty_print s)
    | Apply(name, args) -> multi_node "Apply" (name + " " + (List.fold (fun acc x -> acc + add_bracket x + " ") "" (List.map pretty_draw args)))
    | Let_Var(name, body, expr) -> multi_node "Let_Var" (name + " " + (pretty_draw body) + (pretty_draw expr))
    | Let_Fun(name, ps, body, expr) -> multi_node "Let_Fun" (name + " " + (pretty_print ps) + (pretty_draw body) + (pretty_draw expr))
    | Match(cond, cases) -> multi_node "Match" ((pretty_draw cond) + (pretty_print (List.map (fun (p, e) -> p.ToString() + ": " + (pretty_draw e)) cases)))
