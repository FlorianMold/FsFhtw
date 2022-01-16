[<EntryPoint>]
let main argv =
    printfn "Welcome to the FHTW Domain REPL!"
    printfn "Please enter your commands to interact with the system."
    printfn "Press CTRL+C to stop the program."
    printf "> "

    let mutable cart = (Domain.init ())
    cart <- Domain.addTicketToCart cart {nr = 1} Domain.AdultTicket 1
    cart <- Domain.addTicketToCart cart {nr = 1} Domain.AdultTicket 2
    cart <- Domain.addTicketToCart cart {nr = 1} Domain.AdultTicket 2

    cart <- Domain.removeTicketFromCart cart {nr = 1} Domain.AdultTicket 2

    cart <- Domain.clearCart cart

//    Repl.loop initialState
    0 // return an integer exit code
