namespace QAL.Library

open QAL.Definitions.TypeDef
open DecimalMath
open QAL.Utils.Error

module Arithmetic =    
    
    let complex_to_decimal c =
        match c with
        | Complex_Val(m, a) -> 
            // if c is positive real
            if a % 2m = 0m then
                Some m
            // if c is negative real
            elif a % 2m = 1m || a % 2m = -1m then
                Some -m
            // if c is complex
            else
                None
        | _ -> invalidArg "complex" "expect complex"
    
    let to_int c =
        match c with
        | Complex_Val(m, a) -> 
            match complex_to_decimal (Complex_Val(m, a)) with
            | Some d -> int (round d)
            | None -> 
                invalidArg "complex" "imaginary part is not 0: ambiguous rounding"
        | Integer_Val i -> i
        | other -> invalidArg "complex" $"cannot cast {other} to integer"
    
    let to_complex num =
        match num with
        | Integer_Val i when i >= 0 -> Complex_Val(decimal i, 0m)
        | Integer_Val i when i < 0 -> Complex_Val(decimal i, 1m)
        | Complex_Val(m, a) -> Complex_Val(m, a)
        | other -> invalidArg "number" $"cannot cast {other} to integer"
    
    let complex_constructor args =
        // convert the modulus and argument to decimal, regardless whether it's an integer or decimal
        match List.map (to_complex >> complex_to_decimal) args with
        | Some m :: Some a :: []  -> Complex_Val(m, a)
        | _ -> invalidArg "modulus and argument" "either one of them is a complex number"
    
    /// adds integers or complexes (including decimals)
    let rec add args =
        match args with
        // integer addition
        | Integer_Val i1 :: Integer_Val i2 :: [] -> Integer_Val(i1 + i2)
        | Complex_Val(m1, a1) :: Complex_Val(m2, a2) :: [] -> 
            let s1, s2 = complex_to_decimal (Complex_Val(m1, a1)), complex_to_decimal (Complex_Val(m2, a2))
            match s1, s2 with
            // real addition
            | Some d1, Some d2 -> 
                if m1 + m2 < 0m then
                    Complex_Val (-d1 - d2, 1m)
                else
                    Complex_Val (d1 + d2, 0m)
            // complex addition
            // why would anyone want to do this ?
            | _, _ -> 
                let argand_to_cartesian m a =
                    m * DecimalEx.Cos (a * DecimalEx.Pi), m * DecimalEx.Sin (a * DecimalEx.Pi)
                let r1, i1 = argand_to_cartesian m1 a1
                let r2, i2 = argand_to_cartesian m2 a2
                let r = r1 + r2
                let i = i1 + i2
                let m = DecimalEx.Sqrt (r * r + i * i)
                let a = DecimalEx.ATan (i / r) / DecimalEx.Pi
                Complex_Val(m, a)
        // general case
        | Complex_Val(m1, a1) :: Integer_Val i :: [] -> 
            add [(Complex_Val(m1, a1)); (to_complex (Integer_Val i))]
        | Integer_Val i :: Complex_Val(m2, a2) :: [] -> 
        // symmetric general case
            add [(to_complex (Integer_Val i)); (Complex_Val(m2, a2))]
        | other1 :: other2 :: [] -> 
            invalidArg "expression 1 or expression 2" $"cannot cast either {other1} or {other2} to integer"
        | _ -> too_many_args_err 2 args
    
    /// <summary>raise the base to a specified power. Same for functions</summary>
    /// <remarks>the power cannot be negative</remarks>
    let pow interp_w_sim args =
        match args with
        | Integer_Val basei :: Integer_Val power :: [] -> 
            let res = DecimalEx.Pow(decimal basei, decimal power)
            Integer_Val (int res)
        | Complex_Val(m, a) :: Integer_Val i :: [] ->
            let m' = DecimalEx.Pow(m, decimal i)
            let a' = DecimalEx.Pow(a, decimal i)
            Complex_Val(m', a')
        | _ -> too_many_args_err 2 args
    
    

