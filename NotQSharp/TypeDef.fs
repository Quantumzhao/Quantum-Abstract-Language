module TypeDef

open Quantum.Simulator
open Microsoft.Quantum
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
    | Apply of function_name: string * arguments: Expr list
    | Let of string * Expr
    | Match of condition: Expr * cases: (Pattern * Expr) list
    /// user defined functions
    | FuncDef of name: string * parameters: string list * body: Expr
    /// standard library functions
    | StdApply of name: string * parameters: Expr list
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
    | Function_Red of string * string list * Expr

