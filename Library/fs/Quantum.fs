namespace QAL.Library

open Microsoft.Quantum.Simulation.Core
open QAL.Utils.Error
open QAL.Utils.Misc
open QAL.QuantumLib
open QAL.Definitions.TypeDef
open Microsoft.Quantum.Simulation.Simulators

module internal Quantum =

    /// allocate a new composite system
    let new_qubits (sim: QuantumSimulator) arg =
        match arg with
        | Integer_Val n :: [] -> 
            // a QArray of Q# qubits
            let q_array = sim.QubitManager.Allocate(int64 n)
            // an F# list of my qubits
            let proc'ed_arr = List.map Qubit_Val (arr_2_lst q_array)
            // a system of my qubits
            System_Val proc'ed_arr
        | _ -> too_many_args_err 1 arg

    /// allocate a single new qubit (possibly an ancilla)
    let new_qubit (sim: QuantumSimulator) args =
        match args with
        | Integer_Val option :: [] -> 
            let qubit = sim.QubitManager.Allocate()
            match option with
            | 0 -> Qubit_Val qubit
            | 1 -> Qubit_Val (XGate.Run(sim, qubit).Result)
            | 2 -> Qubit_Val (HGate.Run(sim, qubit).Result)
            | 3 -> 
                let q1 = XGate.Run(sim, qubit).Result
                Qubit_Val (HGate.Run(sim, q1).Result)
            | _ -> invalidArg "option" "no, it can't, it's surreal"
        | _ -> too_many_args_err 1 args

    /// measures the qubit, and returns the result together with a new state of the qubit
    let measure_n_reset sim args =
        match args with
        | Qubit_Val q :: [] -> 
            let struct(res, qubit) = Measure.Run(sim, q).Result
            if res = Result.One then Tuple_Val[Integer_Val 1; Qubit_Val qubit]
            else Tuple_Val[Integer_Val 0; Qubit_Val qubit]
        | _ -> too_many_args_err 1 args

    /// the classical measure operation. Can only return the classical result
    let measure sim args =
        match measure_n_reset sim args with
        | Tuple_Val(res :: _) -> res
        | other -> syntax_err Tuple_Val other

    let basic_gate sim code arg =
        match arg with
        // 1 qubit gates
        | Qubit_Val q :: [] -> 
            match code with
            | "H" -> Qubit_Val (HGate.Run(sim, q).Result)
            | "X" -> Qubit_Val (XGate.Run(sim, q).Result)
            | "Y" -> Qubit_Val (YGate.Run(sim, q).Result)
            | "Z" -> Qubit_Val (ZGate.Run(sim, q).Result)
            | _ -> invalidArg "code" "no, it can't, it's surreal"
        // 2 qubit gates
        | Qubit_Val ctl :: Qubit_Val tgt :: [] -> 
            match code with
            | "CNOT" -> 
                let struct(ctl', tgt') = ControlledXGate.Run(sim, ctl, tgt).Result
                Tuple_Val [Qubit_Val ctl'; Qubit_Val tgt']
            | _ -> invalidArg "code" "no, it can't, it's surreal"
        | _ -> too_many_args_err 1 arg
