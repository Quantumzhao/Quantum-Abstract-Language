module TokDef

type Token =
    | LParen
    | RParen
    | Let
    | In
    | Match
    | With
    | VBar
    | Underline
    | Arrow
    | Equal
    | Comma
    | Integer of int
    | Decimal of decimal
    | Identifier of string
    | String of string
