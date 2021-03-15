# To Discuss

- [X] ownership

  > ok
  
- [X] aliasing/borrowing

  > ok
  
- high order function (`map`, `filter`, etc.)

- array vs. tuple (composite system) notation

- ⊕ `xor` (high/low level? Quantum Fourier Transformation? )

- boolean

  > No

- indexing

- anonymous function

- user defined function

- representation of tensor product

- standard library

- recursion/mutually recursive

- currying

- typing (explicit/implicit)

- grammar style

- claiming and disposing quantum resources

- [X] "cloning" qubit

  > don't use it
  
- name

  > probably `F##Q`

- [X] call by name

  > probably call-by-value

- deconstructor

- pattern matching

# TODO

# Example code

See `./Example/test.txt`

# Notes

The overall syntax is Lisp-like. 

We use (F# documentation)[https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/xml-documentation]

## Data Structure

There are 3 types of collections in this language: 

|                           | Tuple | Array | Composite system |
| ------------------------- | ----- | ----- | ---------------- |
| Supports indexing         | No    | Yes   | No               |
| Supports iteration        | No    | Yes   | Yes              |
| Supports qubit            | Yes   | No    | Yes              |
| Supports classical data   | Yes   | Yes   | No               |
| Supports pattern matching | Yes   | No    | No               |

In addition, there are differences between classical data and qubit: 

|                    | Classical data | Qubit |
| ------------------ | -------------- | ----- |
| Unitary operation  | Undefined      | Yes   |
| Classical function | Yes            | No    |
| Re-useable         | Yes            | No    |

## Functions

### Unitary operations

### Classical functions
