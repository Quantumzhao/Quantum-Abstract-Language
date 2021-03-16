module TypeDef

open Microsoft.Quantum.Simulation.Core

type Pattern =
    | Placeholder of string
    | WildCard

type Expr = 
    | Integer of int
    | Complex of modulus: decimal * argument: decimal
    // probably useless
    | String of string
    | Unit
    /// normal values, qubits and functions
    | Variable of string
    /// Note that the function 
    /// can also be a return value from an expression
    | Apply of func: Expr * arguments: Expr list
    /// let binding of a variable
    | Let_Var of name: string * body: Expr * In: Expr
    /// let binding of a function
    | Let_Fun of name: string * paras: string list * body: Expr * In: Expr
    | Match of condition: Expr * cases: (Pattern * Expr) list
    | Array of Expr list
    /// fixed number of expressions, supports pattern matching
    | Tuple of Expr list
    /// can only contain qubits
    | System of Expr list
    | Qubit of Qubit

type Value = 
    | Unit_Val
    | String_Val of string
    | Complex_Val of decimal * decimal
    | Integer_Val of int
    | Array_Val of Value list
    /// value can only be qubits
    | System_Val of Value list
    | Tuple_Val of Value list
    | Qubit_Val of Qubit
    /// function as a value (include complete definition and body)
    | Function_Red of string * string list * Expr
