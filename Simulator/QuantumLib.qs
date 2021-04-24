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

    // TODO: controlled hadamard use the cont
    operation ControlledHGate (control: Qubit, target: Qubit) 
    : (Qubit, Qubit) {
        //CH(control, target);
        return (control, target);
    }
    // Also please follow the naming conventions and function signatures
    //for the adjoint the notation on the Gates are hard
    // I dont know how I will incorporate it here. 
    // i checked on the docs.microsoft.com i dont see it
    operation XGate (target: Qubit) : Qubit {
        X(target);
        return target;
    }

    operation ControlledXGate (control: Qubit, target: Qubit) 
    : (Qubit, Qubit) {
        CX(control, target);
        return (control, target);
    }

    operation YGate (target: Qubit) : Qubit {
        Y(target);
        return target;
    }

    // TODO: controlled Y
    operation ControlledYGate(control: Qubit, target: Qubit)
    :   (Qubit, Qubit) {
        CY(control, target);
        return (control, target);
    }

    // TODO: adjoint Y
    operation AdjointY (target: Qubit) : Qubit {
        Y(target);
        return target;
    }
    // ZGate
    operation ZGate (target: Qubit) : Qubit {
        Z(target);
        return target;
    }

    // TODO: controlled Z
    operation ControlledZGate(control: Qubit, target: Qubit)
    :   (Qubit, Qubit) {
        CZ(control, target);
        return (control, target);
    }

    // TODO: adjoint Z
    operation AdjointZ (target: Qubit) : Qubit {
        Z(target);
        return target;
    }

    // TODO: phase (S gate)
     operation SGate (target: Qubit) : Qubit {
        S(target);
        return target;
    }

    // TODO: controlled phase
    operation ControlledSGate(control: Qubit, target: Qubit)
    :   (Qubit, Qubit) {
        //CS(control, target);
        return (control, target);
    }

    // TODO: adjoint phase
    operation AdjointS (target: Qubit) : Qubit {
        S(target);
        return target;
    }

    // TODO: π/8 gate
     operation TGate (target: Qubit) : Qubit {
        T(target);
        return target;
    }

    // TODO: controlled π/8 gate
    operation ControlledTGate(control: Qubit, target: Qubit)
    :   (Qubit, Qubit) {
        //CT(control, target);
        return (control, target);
    }

    // TODO: adjoint π/8 gate
    operation adjointT (target: Qubit) : Qubit {
        T(target);
        return target;
    }

    // TODO: controlled on bit string
    //I look over don see it the symbol
    operation ControlledOnBitStringGate(control: Qubit, target: Qubit)
    :   (Qubit, Qubit) {
        //COBS(control, target);
        return (control, target);
    }
}
