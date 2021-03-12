// Learn more about F# at http://fsharp.org

open Microsoft.Quantum.Simulation.Core
open Microsoft.Quantum.Simulation.Simulators
open System

[<EntryPoint>]
let main argv =
    use sim = new QuantumSimulator()
    let qs = sim.QubitManager.Allocate 2L
    let ctl = qs.[0]
    let tgt = qs.[1]
    sim.H__Body qs.[0]
    sim.X__ControlledBody (new QArray<Qubit>(ctl), tgt)
    let res1 = sim.Measure__Body(new QArray<Pauli>(Pauli.PauliZ), new QArray<Qubit>(qs.[0]))
    let res2 = sim.Measure__Body(new QArray<Pauli>(Pauli.PauliZ), new QArray<Qubit>(qs.[1]))
    Console.WriteLine(res1.ToString() + " " + res2.ToString())
    Console.WriteLine "Hello World from F#!"
    0 // return an integer exit code
