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

init(H, n)

map(gates(n), (fun g -> H)
map(gatesi(n), (fun (g, i) -> H^i)
array1[0]
<H . H> (H . H)

map <q1 . q2> H

let xxx = if (i % 2 == 0) H(q) else q

fori qs xxx
q0 -H-
q1 ---
q2 -H-
...