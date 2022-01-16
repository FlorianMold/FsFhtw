[<EntryPoint>]
let main argv =
    printfn "Welcome to the FHTW Domain REPL!"
    printfn "Please enter your commands to interact with the system."
    printfn "Press CTRL+C to stop the program."
    printf "> "

    let initialState = Domain.init ()
    let r = Domain.requestTicket {name = "Vienna"} {name = "Linz"}
    printfn $"%A{r}"
    Repl.loop initialState
    0 // return an integer exit code
