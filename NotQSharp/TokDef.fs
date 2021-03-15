module TokDef

type Token =
    | LParen
    | RParen
    | Let
    | In
    | Match
    | With
    | Equal
    | Comma
    | Integer of int
    | Decimal of decimal
    | Identifier of string
