namespace QAL.Utils

open QAL.Definitions.TypeDef

module Error =

    let not_implemented_err () = 
        failwith "not implemented"

    let syntax_err expect actual =
        failwith $"syntax error: expect {expect}, actual {actual}"

    let no_such_element_err arg collection =
        failwith $"cannot find {arg} in {collection}"

    let too_many_args_err (expect: int) (actual_arg_list: Value list) =
        syntax_err $"{expect} args" $"{actual_arg_list.Length} args"
