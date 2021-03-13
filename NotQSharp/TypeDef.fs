module TypeDef

open Quantum.Simulator
open Microsoft.Quantum
open Microsoft.Quantum.Simulation.Core

type Expr = 
    | Integer of int
    | Complex of modulus: decimal * argument: decimal
    | String of string
    | Unit
    | Variable of string
    | Apply of function_name: string * arguments: Expr list
    | Let of string * Expr
    /// <summary>user defined functions</summary>
    | Function of name: string * parameters: string list * body: Expr
    /// <summary>standard library functions</summary>
    | StdApply of name: string * parameters: Expr list
    | Array of Expr list
    | Qubit of Qubit

type Value = 
    | Unit_Val
    | String_Val of string
    | Complex_Val of decimal * decimal
    | Integer_Val of int
    | Array_Val of Value list
    | Qubit_Val of Qubit

