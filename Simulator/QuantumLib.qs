namespace QuantumLib {

    open Microsoft.Quantum.Canon;
    open Microsoft.Quantum.Intrinsic;
    
    /// # Summary
    /// all qubits are initialized as |0⟩
    /// # Input
    /// ## number
    /// number of qubits
    operation ClaimQubits (number: Int) : Qubit[] {
        return new Qubit[number];
    }

    /// # Summary
    /// initializes a single qubit
    /// # Input
    /// ## option
    /// - 0: |0⟩
    /// - 1: |1⟩
    /// - 2: |+⟩
    /// - 3: |-⟩
    operation ClaimQubit (option: Int) : Qubit {
        use q = Qubit();
        if (option == 0) {
            I(q);
        } elif (option == 1) {
            X(q);
        } elif (option == 2) {
            H(q);
        } elif (option == 3) {
            X(q);
            H(q);
        }
        return q;
    }

    operation Measure (target: Qubit) : (Result, Qubit) {
        return (M(target), target);
    }

    operation HGate (target : Qubit) : Qubit {
        H(target);
        return target;
    }

    // TODO: controlled hadamard
    // Also please follow the naming conventions and function signatures

    operation XGate (target: Qubit) : Qubit {
        X(target);
        return target;
    }

    operation ControlledXGate (control: Qubit, target: Qubit) 
    : (Qubit, Qubit) {
        CNOT(control, target);
        return (control, target);
    }

    operation YGate (target: Qubit) : Qubit {
        Y(target);
        return target;
    }

    // TODO: controlled Y

    // TODO: adjoint Y

    operation ZGate (target: Qubit) : Qubit {
        Z(target);
        return target;
    }

    // TODO: controlled Z

    // TODO: adjoint Z

    // TODO: phase (S gate)

    // TODO: controlled phase

    // TODO: adjoint phase

    // TODO: π/8 gate

    // TODO: controlled π/8 gate

    // TODO: adjoint π/8 gate

    // TODO: controlled on bit string
}
