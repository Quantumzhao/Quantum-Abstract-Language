# To Discuss

Needs inquiry:

- how to verify a unitary operation programmatically
- why Q# default `M` measures in Z not I

# TODO

- [x] a fully functional lexer
  - [x] support line comment
  - [x] support string
  - [x] full support of symbol lexing
- [x] parser
  - [X] apply
  - [X] tuple
  - [X] tuple, apply without Paren
  - [X] support string
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

- [X] indexing

  > use `index` for only arrays

- [x] representation of tensor product

  > - no literal. use `System` function (constructor) to construct a composite system. 
  >
  > - tensor product of unitary matrices is not supported (from syntax level)

- [x] recursion/mutually recursive

  > No for now. Very likely yes in the future

- [x] grammar style

  > CAML style

- [X] claiming and disposing quantum resources

  > do it

- [X] "cloning" qubit (creating entangling qubits with the same state)

  > don't use it
  
- [x] name

  > probably QAML

- [X] call by name

  > no, call-by-value

## Introduction

This is a quantum functional programming language, inspired by the theory of Quantum Lambda Calculus developed by Sir Selinger. Since it is a proof-of-concept language experimenting some novel ideas present in the realm, we decide to preserve its simplicity by removing all syntactic sugar and non-critical syntax support that are common to many existing functional PLs. 

For example, the following common features are missing: 

- anonymous function

  > Every function should be declared with a name

- partial application

  > You must use a wrapper function to apply some of the arguments

- Boolean data type

  > `0` means `false` and `1` means `true`

- type system

  > Types still exist, but there is no way of explicitly declaring a variable as a certain type. Also, application and function signatures don't enforce type checking

- pattern matching

  > Actually it supports a subset of CAML style pattern matching

However, it does have good support for quantum data types and qubit operations, which will be discussed in detail in the following sections. 

We use [F# XML documentation](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/xml-documentation)

### No-cloning Theorem

Despite the syntax being similar to classical ML style language, our language is fundamentally different from these. 

Due to the presence of no-cloning theorem, we have to redesign even the most basic data structures (for example, tuples and arrays) in order to circumvent the constrains and keep the functional programming semantics in many functions and high order functions. 

For instance, consider the following code snippet: 

```F#
let qubit' = qubit in
...
```

Unlike classical data assignment, `qubit` becomes inaccessible after this operation. The reason is that we have to keep track of the references of qubit variables (pointers), so that no quantum variables may point to the same quantum resource. 

Otherwise,  disastrous scenarios like this may happen:

```F#
let q = qubit in
	CNOT q qubit
```

The `CNOT` gate operates on two same qubits which is unthinkable in quantum computing. 

Or something slightly less worse: 

```F#
let qubit' = X qubit in
let qubit'' = qubit in
...
```

Since `qubit ` value has been changed since line 1, another access may get a different value than the origin value of `qubit`. This is especially unacceptable in functional programming since it implies implicit mutable variables. 

Therefore we take the approach of linearity, which means no variables are allowed to use twice. 

> Note that this is quite different as what descried in Sir Selinger's *Quantum Lambda Calculus* and *Quipper* language, because we think creating an entangled pair with the same state to immitate the "cloning" operation is not a good idea. 

This leads to a profound shift in our language design. 

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




