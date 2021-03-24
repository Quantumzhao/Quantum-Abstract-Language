# To Discuss

- [X] ownership

  > yes
  
- [X] aliasing/borrowing

  > - aliasing: yes
  > - borrowing: add a new standard library function `borrow`
  
- [x] high order function (`map`, `filter`, etc.)

  > `map`, `fold`
  >
  > also use `range` to generate a set of indices. classical `for` loop can be achieved via `fold` + `range`

- [x] array vs. tuple (composite system) notation

  > use composite system and tuple for qubits, array for classical data

- [x] ⊕ `xor` (high/low level? Quantum Fourier Transformation? )

  > high level, consistent with Q#

- [x] boolean

  > No, use integer `0` and anything instead

- [X] indexing

  > use `index` for only arrays

- [x] anonymous function

  > No

- [x] user defined function

  > Yes

- [x] representation of tensor product

  > - no literal. use `System` function (constructor) to construct a composite system. 
  >
  > - tensor product of unitary matrices is not supported (from syntax level)

- [x] recursion/mutually recursive

  > No for now. Very likely yes in the future

- [x] currying

  > Partial application is not allowed. Use wrappers instead

- [x] typing (explicit/implicit)

  > No explicit typing. 

- [x] grammar style

  > CAML style

- [ ] claiming and disposing quantum resources

- [X] "cloning" qubit (creating entangling qubits with the same state)

  > don't use it
  
- [x] name

  > probably F##Q

- [X] call by name

  > no, call-by-value

- [ ] deconstructor

  > yes for tuple in `match` block, but needs further discussion

- [ ] pattern matching

  > yes for tuple in `match` block, but needs further discussion

Needs inquiry:

- whether to implement adjoint variant
- how to verify a unitary operation programmatically
- Hadamard sandwich
- why Q# default `M` measures in Z not I

# TODO

- [ ] a fully functional lexer
  - [ ] support line comment
  - [ ] support string
  - [x] full support of symbol lexing
  - [ ] no need for space bewteen tokens
  - [ ] regexp support for identifier with underline and numbers
- [ ] parser
  - [ ] apply
  - [ ] tuple
  - [x] other
- [ ] interpreter
  - [ ] qubit management
  - [ ] `controlled` implementation
  - [ ] `adjoint` implementation
  - [x] other
- [ ] standard library
  - [ ] some necessary functions

# Example code

See `./Example`

# Notes

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

- Arrays cannot include qubits because the no-cloning theorem: Once the qubit system (a subset of the array) evolves via some unitary matrices, the new state exists in the form of function return value and is no longer a subset of the array. This will leave holes in the sequenced data structure, invalidating future enumeration operations. 
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

- the pattern should not be wrapped in parenthesis. 

- the pattern is not recursive. For example: 

  ```f#
  match tuple with
  | (_, _), _ -> 0
  ```

  Is illegal




