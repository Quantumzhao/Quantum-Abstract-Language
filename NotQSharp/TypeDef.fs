module TypeDef

open System
open Quantum.Simulator
open Microsoft.Quantum
open Microsoft.Quantum.Simulation.Core

type CExpr = 
    | Integer of int
    | Variable of Variable
    | Prim1 of string * CExpr
    | Prim2 of string * CExpr * CExpr
    | App of string * CExpr * CExpr
    | Let of Binding * CExpr

and FunDef = Variable * Variable list * Expr

and Binding = Variable * Expr

and Variable = Symbol of string

and QExpr = QubitExpr of Qubit

and Expr = 
    | CExpr of CExpr
    | QExpr of QExpr
