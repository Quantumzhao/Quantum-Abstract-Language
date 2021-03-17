# To Discuss

- [X] ownership

  > ok
  
- [X] aliasing/borrowing

  > ok
  
- [x] high order function (`map`, `filter`, etc.)

  > `map`, `fori`

- [x] array vs. tuple (composite system) notation

  > use composite system (`<q1 . q2>`) and tuple for qubits, array for classical data

- [x] ⊕ `xor` (high/low level? Quantum Fourier Transformation? )

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

Needs inquiry:

- whether to implement adjoint variant
- how to verify a unitary operation programmatically
- Hadamard sandwich
- why Q# measures in Z not I

# TODO

- [ ] a fully functional lexer
  - [ ] support line comment
  - [ ] support string
  - [ ] full support of symbol lexing

# Example code

See `./Example/test.txt`

# Notes

The overall syntax is ML-like. 

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

|                        | Classical data | Qubit |
| ---------------------- | -------------- | ----- |
| Unitary transformation | Undefined      | Yes   |
| Classical function     | Yes            | No    |
| Re-useable             | Yes            | No    |

Some explanations: 

- qubits are not re-usable due to no-cloning theorem. For example, 

  ```F#
  let q = qubit in
  CNOT q qubit
  ```

  This will be a horrible error. 

## Blocks

### `let` Binding

For the formal definition of this block, please see the source code. 

`let` can be in one of the following forms: 

#### Single variable binding

```F#
let x = 0 in
...
```

#### Function declaration

```F#
let id x = x in
...
```

#### Tuple deconstruction and multi-variable binding

The basic form is like this:

```F#
let a, b = tuple in
...
```

However, it is also possible to use wildcard

```f#
let a, _ = tuple in
...
```

And even further: 

```f#
let _, _ = tuple in
...
```

> Note that the `in` keyword cannot be omitted. 
>
> Also, the following syntax is considered invalid: 
>
> ```f#
> let (a, b) = tuple in
> ...
> ```

### Pattern matching

Apart from the aforementioned tuple deconstruction in `let` binding, it is also possible to use pattern matching in `match` block. 

For the formal definition of this block, please see the source code. 

```f#
match tuple with
| 0, b, c -> b
| a, 0, _ -> a
| _ -> 0
```



Note that the language does not enforce type check, so it is possible to write the following code, although its behavior is undefined: 

```f#
match tuple with
| 1, 2 -> 1
| a, _, _ -> a
```

It serves the functionality of `if` block: 

```f#
match to_bool num with
| 0 -> 0
| 1 -> 1
```

In which case, `to_bool` function has the following definition: 
$$
\text{to_bool}:\Z\mapsto\{0,1\} \\
\text{to_bool}(x)=
\begin{cases}
0&\text{ if x=0}\\
1&\text{ otherwise}
\end{cases}
$$
There are some peculiarities: 

- the language only supports the pattern matching of **tuple** and **scalar values**

- the pattern should not wrapped in parenthesis. 

- the pattern is not recursive, that is, 

  ```f#
  match tuple with
  | (_, _), _ -> 0
  ```

  Is illegal

- 

