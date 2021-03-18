namespace QuantumLib {

    open Microsoft.Quantum.Canon;
    open Microsoft.Quantum.Intrinsic;
    
    //@EntryPoint()
    operation HelloQ () : Unit {
        Message("Hello quantum world!");
    }

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

    operation Hadamard(target : Qubit) : Qubit {
        H(target);
        return target;
    }

    operation Pauli_X(target: Qubit) : Qubit {
        X(target);
        return target;
    }

    operation Pauli_Z(target: Qubit) : Qubit {
        Z(target);
        return target;
    }

    operation Measure(target: Qubit) : Result {
        return M(target);
    }
}
