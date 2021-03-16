# To Discuss

- [X] ownership

  > ok
  
- [X] aliasing/borrowing

  > ok
  
- high order function (`map`, `filter`, etc.)

  > `map`, `fori`

- array vs. tuple (composite system) notation

  > use composite system (`<q1 . q2>`) and tuple for qubits, array for classical data

- ⊕ `xor` (high/low level? Quantum Fourier Transformation? )

- boolean

  > No

- [X] indexing

  > Normal style

- anonymous function

  > No

- user defined function

  > Yes

- representation of tensor product

  > for `n <= 2`, use `<q1 . q2>`, otherwise use `map` orother collection ops

- recursion/mutually recursive

  > No for now

- currying

  > No

- typing (explicit/implicit)

  > No

- grammar style

  > Racket style

- claiming and disposing quantum resources

- [X] "cloning" qubit

  > don't use it
  
- name

  > probably F##Q

- [X] call by name

  > probably call-by-value

- deconstructor

- pattern matching

# TODO

# Example code

See `./Example/test.txt`

# Notes

The overall syntax is Lisp-like. 

We use [F# XML documentation](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/xml-documentation)

## Data Structure

There are 3 types of collections in this language: 

|                           | Tuple | Array | Composite system |
| ------------------------- | ----- | ----- | ---------------- |
| Supports indexing         | No    | Yes   | No               |
| Supports iteration        | No    | Yes   | Yes              |
| Supports qubit            | Yes   | No    | Yes              |
| Supports classical data   | Yes   | Yes   | No               |
| Supports pattern matching | Yes   | No    | No               |
| Supports literal          | Yes   | No    | No               |

Some explanations: 

- Arrays cannot include qubits because the no-cloning theorem: Once the qubit evolves via some unitary matrices, the new state exists in the form of function return value and is no longer an element of the array. This will leave a hole in the sequenced data structure, invalidating future enumeration operations. 
- Composite system cannot include classical data because it is a data structure specialized for quantum state evolvement. 

In addition, there are differences between classical data and qubit: 

|                    | Classical data | Qubit |
| ------------------ | -------------- | ----- |
| Unitary operation  | Undefined      | Yes   |
| Classical function | Yes            | No    |
| Re-useable         | Yes            | No    |

Some explanations: 

- qubits are not re-usable due to no-cloning theorem. For example, 

  ```F#
  let q = qubit in
  CNOT q qubit
  ```

  This will be a horrible error. 