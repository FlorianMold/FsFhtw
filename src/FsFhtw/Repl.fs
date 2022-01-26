module Repl

open System
open Domain
open Parser

type Message =
    | DomainMessage of Domain.Message
    | HelpRequested
    | NotParsable of string

type State = Domain.State

let read (input: string) =
    match parseInput input with
    | PrintTrainStations -> Domain.PrintTrainStations |> DomainMessage
    | SearchTrips st -> Domain.SearchTrips st |> DomainMessage
    | RequestTicket st -> Domain.RequestTicket st |> DomainMessage
    | AddTicketToCart cm -> Domain.AddTicketToCart cm |> DomainMessage
    | PrintCart -> Domain.PrintCart |> DomainMessage
    | RemoveTicketFromCart cm -> Domain.RemoveTicketFromCart cm |> DomainMessage
    | ClearCart -> Domain.ClearCart |> DomainMessage
    | ShowPaidOrders -> Domain.ShowPaidOrders |> DomainMessage
    | PayCart pm -> Domain.PayCart pm |> DomainMessage
    | Help -> HelpRequested
    | ParseFailed -> NotParsable input

open Microsoft.FSharp.Reflection

let createHelpText () : string =
    FSharpType.GetUnionCases typeof<Domain.Message>
    |> Array.map (fun case -> case.Name)
    |> Array.fold (fun prev curr -> prev + " " + curr) ""
    |> (fun s -> s.Trim() |> sprintf "Known commands are: %s")

// TODO(Aytac): write the printing
// TODO(Aytac): Bei Operationen, die das Manipulieren des Carts betrifft werden wir den alten cart mit dem
// TODO(Aytac): neuen cart vergleichen muesen, damit wir feststellen koennen, dass sich nicht veraendert hat.
// TODO(Aytac): e.g.: addTocart mit nicht exister ticket-id...

let dateFormat = "dd.MM.yyyy hh:mm"

let trainStationsToString (trainStations: list<Domain.TrainStation>) = 
    printfn "--------------------------------------------"
    for trainStation in trainStations 
        do printfn "%s " trainStation.name;
    printfn "--------------------------------------------"

let searchTripsToString (tickets: list<Domain.Ticket>) = 
    printfn "| Ticket-nr |   From   |   To   |      Departuretime - Arrivaltime      |   Train Type   |"
    printfn "-----------------------------------------------------------------------------------------"
    for ticket in tickets 
        do printfn "|    %d    |  %s  |  %s  | %s  -  %s | %O |" 
            ticket.ticketNr 
            ticket.departure.name 
            ticket.arrival.name 
            (ticket.departureTime.ToString dateFormat) 
            (ticket.arrivalTime.ToString dateFormat)  
            ticket.trainType;
    printfn "-----------------------------------------------------------------------------------------";

let requestTicketToString (tickets: list<Domain.Ticket>) = 
    printfn "| Ticket-nr |   From   |   To   |      Departuretime - Arrivaltime      |     Train Type    |     Ticket Type    |"
    printfn "----------------------------------------------------------------------------------------------------------------"
    for ticket in tickets 
        do printfn "|    %d    |  %s  |  %s  | %s  -  %s | %O | %O |" 
            ticket.ticketNr 
            ticket.departure.name 
            ticket.arrival.name 
            (ticket.departureTime.ToString dateFormat) 
            (ticket.arrivalTime.ToString dateFormat)  
            ticket.trainType
            ticket.ticketType;
    printfn "----------------------------------------------------------------------------------------------------------------"

let ticketOutput (shoppingCartEntry: list<ShoppingCartEntry>) = 
    printfn "Cart: "
    printfn "| Ticket-nr |   From   |   To   |      Departuretime - Arrivaltime      |    Train Type   |    Ticket Type    | Quantity |"
    printfn "-------------------------------------------------------------------------------------------------------------------------"
    for ticket in shoppingCartEntry 
        do printfn "|    %d    |  %s  |  %s  | %s  -  %s |     %O    |   %O   |     %d    |" 
            ticket.ticket.ticketNr
            ticket.ticket.departure.name 
            ticket.ticket.arrival.name 
            (ticket.ticket.departureTime.ToString dateFormat) 
            (ticket.ticket.arrivalTime.ToString dateFormat)  
            ticket.ticket.trainType
            ticket.ticket.ticketType
            ticket.quantity;
    printfn "-------------------------------------------------------------------------------------------------------------------------";

let unpaidCartToString (oldCart: Domain.UnpaidCart) (newCart: Domain.UnpaidCart) = 
   if oldCart.tickets.Length = newCart.tickets.Length 
    then 
        printfn "Es gab keine Änderungen"
        ticketOutput oldCart.tickets
    else
        ticketOutput newCart.tickets

let loopPaidCart paidCart = 
    for ticket in paidCart.tickets 
            do printfn "|    %d    |  %s  |  %s  | %s  -  %s |     %O    |   %O   |     %d    |     %s    |     %O    |" 
                ticket.ticket.ticketNr
                ticket.ticket.departure.name 
                ticket.ticket.arrival.name 
                (ticket.ticket.departureTime.ToString dateFormat) 
                (ticket.ticket.arrivalTime.ToString dateFormat)  
                ticket.ticket.trainType
                ticket.ticket.ticketType
                ticket.quantity
                (ticket.orderDate.ToString dateFormat)
                ticket.paymentMethod;

let paidCartToString (paymentResult: Domain.PaymentResult) = 
    
    match paymentResult with
        | Success x ->  
            printfn "| Ticket-nr |   From   |   To   |      Departuretime - Arrivaltime      |    Train Type   |    Ticket Type    | Quantity | Order Date | Payment Method |"
            printfn "--------------------------------------------------------------------------------------------------------------------------------------------------------"
            loopPaidCart x
            printfn "--------------------------------------------------------------------------------------------------------------------------------------------------------"

        | Bounce x -> printfn "%O" x

let paidOrdersToString (paidCarts: list<PaidCart>) = 
    printfn "| Ticket-nr |   From   |   To   |      Departuretime - Arrivaltime      |    Train Type   |    Ticket Type    | Quantity | Order Date | Payment Method |"
    printfn "--------------------------------------------------------------------------------------------------------------------------------------------------------"
    
    for paidCart in paidCarts 
        do loopPaidCart paidCart
    printfn "--------------------------------------------------------------------------------------------------------------------------------------------------------"


let evaluate (update: Domain.Message -> State -> State) (state: State) (msg: Message) =
    let (oldCart, _, _, _, _) = state

    match msg with
    | DomainMessage msg ->
        let newState = update msg state
        let (cart, ticketList, stations, paymentResult, paidOrders) = newState

        let message =
            match msg with
            | Domain.PrintTrainStations -> trainStationsToString stations
            | Domain.SearchTrips _ -> searchTripsToString ticketList
            | Domain.RequestTicket _ -> requestTicketToString ticketList
            | Domain.AddTicketToCart _ -> unpaidCartToString oldCart cart
            | Domain.PrintCart -> unpaidCartToString oldCart cart
            | Domain.RemoveTicketFromCart _ -> unpaidCartToString oldCart cart
            | Domain.ClearCart -> unpaidCartToString oldCart cart
            | Domain.PayCart _ -> paidCartToString paymentResult
            | Domain.ShowPaidOrders -> paidOrdersToString paidOrders

        (newState)
    | HelpRequested ->
        printfn "%s" (createHelpText ())

        (state)
    | NotParsable originalInput ->
        printfn """"%s" was not parsable. %s"""
            originalInput
            "You can get information about known commands by typing \"Help\""

        (state)

let print (state: State) =
    printf "> "
    
    state

let rec loop (state: State) =
    Console.ReadLine()
    |> read
    |> evaluate Domain.update state
    |> print
    |> loop
