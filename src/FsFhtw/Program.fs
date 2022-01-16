[<EntryPoint>]
let main argv =
    printfn "Welcome to the FHTW Domain REPL!"
    printfn "Please enter your commands to interact with the system."
    printfn "Press CTRL+C to stop the program."
    printf "> "

    let initialState = Domain.init ()
    let mutable cart = (Domain.emptyCart ())
    cart <- Domain.addTicketToCart cart {nr = 1} Domain.AdultTicket 1
    cart <- Domain.addTicketToCart cart {nr = 1} Domain.SeniorTicket 2
    printfn $"%A{cart}"

    cart <- Domain.removeTicketFromCart cart {nr = 1} Domain.JuniorTicket 2
    printfn $"%A{cart}"

    Repl.loop initialState
    0 // return an integer exit code
