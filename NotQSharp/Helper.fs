module Helper

let not_implemented_err () = 
    failwith "not implemented"

let syntax_err expect actual =
    failwith $"syntax error: expect {expect}, actual {actual}"
