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

let dateFormat = "dd.MM.yyyy hh:mm"

let trainStationsToString (trainStations: list<Domain.TrainStation>) =
    printfn "------------------------------"

    for trainStation in trainStations do
        printfn "     %s " trainStation.name

    printfn "------------------------------"

let searchTripsToString (tickets: list<Domain.Ticket>) =
    printfn "| Ticket-nr |    From     |     To      |   Price   |      Departuretime - Arrivaltime      |    Train Type   |"
    printfn "---------------------------------------------------------------------------------------------------------------"

    for ticket in tickets do
        printfn
            "|%s|%s|%s|%s|%s - %s|%s|"
            (ticket.ticketNr.ToString().PadLeft(7).PadRight(11))
            (ticket.departure.name.PadLeft(11).PadRight(13))
            (ticket.arrival.name.PadLeft(11).PadRight(13))
            ((sprintf "%.2f $" ticket.ticketPrice).PadLeft(9).PadRight(11))
            ((ticket.departureTime.ToString dateFormat).PadLeft(18))
            ((ticket.arrivalTime.ToString dateFormat).PadRight(18))
            ((sprintf "%O" ticket.trainType).PadLeft(11).PadRight(17))

    printfn "---------------------------------------------------------------------------------------------------------------"


let requestTicketToString (tickets: list<Domain.Ticket>) =
    printfn "| Ticket-nr |    From     |     To      |   Price   |      Departuretime - Arrivaltime      |    Train Type   |   Ticket Type   |"

    printfn
        "-------------------------------------------------------------------------------------------------------------------------------------"

    for ticket in tickets do
        printfn
            "|%s|%s|%s|%s|%s - %s|%s|%s|"
            (ticket.ticketNr.ToString().PadLeft(7).PadRight(11))
            (ticket.departure.name.PadLeft(11).PadRight(13))
            (ticket.arrival.name.PadLeft(11).PadRight(13))
            ((sprintf "%.2f $" ticket.ticketPrice).PadLeft(9).PadRight(11))
            ((ticket.departureTime.ToString dateFormat).PadLeft(18))
            ((ticket.arrivalTime.ToString dateFormat).PadRight(18))
            ((sprintf "%O" ticket.trainType).PadLeft(11).PadRight(17))
            ((sprintf "%O" ticket.ticketType).PadRight(15).PadLeft(17))

    printfn
        "-------------------------------------------------------------------------------------------------------------------------------------"

let ticketOutput (shoppingCartEntry: list<ShoppingCartEntry>) =
    printfn "Cart: "
    
    printfn "| Ticket-nr |    From     |     To      |   Price   |      Departuretime - Arrivaltime      |    Train Type   |   Ticket Type   | Quantity |"

    printfn "--------------------------------------------------------------------------------------------------------------------------------------------"

    for ticket in shoppingCartEntry do
        printfn
            "|%s|%s|%s|%s|%s - %s|%s|%s|%s|"
            (ticket.ticket.ticketNr.ToString().PadLeft(7).PadRight(11))
            (ticket.ticket.departure.name.PadLeft(11).PadRight(13))
            (ticket.ticket.arrival.name.PadLeft(11).PadRight(13))
            ((sprintf "%.2f $" ticket.ticket.ticketPrice).PadLeft(9).PadRight(11))
            ((ticket.ticket.departureTime.ToString dateFormat).PadLeft(18))
            ((ticket.ticket.arrivalTime.ToString dateFormat).PadRight(18))
            ((sprintf "%O" ticket.ticket.trainType).PadLeft(11).PadRight(17))
            ((sprintf "%O" ticket.ticket.ticketType).PadRight(15).PadLeft(17))
            (ticket.quantity.ToString().PadLeft(5).PadRight(10))

    printfn
        "--------------------------------------------------------------------------------------------------------------------------------------------"

let unpaidCartToString (newCart: Domain.UnpaidCart) =
    match newCart.tickets with
    | [] -> printfn "Cart is empty!"
    | _ -> ticketOutput newCart.tickets

let loopPaidCart paidCart =
    for ticket in paidCart.tickets do
        printfn
             "|%s|%s|%s|%s|%s - %s|%s|%s|%s|%s|%s|"
            (ticket.ticket.ticketNr.ToString().PadLeft(7).PadRight(11))
            (ticket.ticket.departure.name.PadLeft(11).PadRight(13))
            (ticket.ticket.arrival.name.PadLeft(11).PadRight(13))
            ((sprintf "%.2f $" ticket.ticket.ticketPrice).PadLeft(9).PadRight(11))
            ((ticket.ticket.departureTime.ToString dateFormat).PadLeft(18))
            ((ticket.ticket.arrivalTime.ToString dateFormat).PadRight(18))
            ((sprintf "%O" ticket.ticket.trainType).PadLeft(11).PadRight(17))
            ((sprintf "%O" ticket.ticket.ticketType).PadRight(15).PadLeft(17))
            (ticket.quantity.ToString().PadLeft(5).PadRight(10))
            ((ticket.orderDate.ToString dateFormat).PadLeft(18).PadRight(20))
            ((sprintf "%O" ticket.paymentMethod).PadRight(12).PadLeft(18))

let paidCartToString (paymentResult: Domain.PaymentResult) =
    printfn "Your Invoice:"
    match paymentResult with
    | Success x ->
        printfn "| Ticket-nr |    From     |     To      |   Price   |      Departuretime - Arrivaltime      |    Train Type   |   Ticket Type   | Quantity |     Order Date     |  Payment Method  |"

        printfn "------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------"

        loopPaidCart x

        printfn "------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------"

        printfn $"Total: %.2f{computeTotalCart x} $"

    | Bounce x -> printfn "%O" x

let paidOrdersToString (paidCarts: list<PaidCart>) =
    printfn "| Ticket-nr |    From     |     To      |   Price   |      Departuretime - Arrivaltime      |    Train Type   |   Ticket Type   | Quantity |     Order Date     |  Payment Method  |"
    
    printfn "------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------"

    for paidCart in paidCarts do
        loopPaidCart paidCart

    printfn "------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------"


let evaluate (update: Domain.Message -> State -> State) (state: State) (msg: Message) =
    let (oldCart, _, _, _, _) = state

    match msg with
    | DomainMessage msg ->
        let newState = update msg state
        let (cart, ticketList, stations, paymentResult, paidOrders) = newState

        let printMessageIfCartHasNotChanged (message: string) =
            if compareCarts oldCart cart then
                printfn $"%s{message}"
            else
                unpaidCartToString cart

        let message =
            match msg with
            | Domain.PrintTrainStations -> trainStationsToString stations
            | Domain.SearchTrips _ when not ticketList.IsEmpty -> searchTripsToString ticketList
            | Domain.RequestTicket _ when not ticketList.IsEmpty -> requestTicketToString ticketList
            | Domain.AddTicketToCart _ -> printMessageIfCartHasNotChanged "Nothing was added to the cart!"
            | Domain.PrintCart -> unpaidCartToString cart
            | Domain.RemoveTicketFromCart _ -> printMessageIfCartHasNotChanged "Nothing was removed to the cart!"
            | Domain.ClearCart -> unpaidCartToString cart
            | Domain.PayCart _ -> paidCartToString paymentResult
            | Domain.ShowPaidOrders when not paidOrders.IsEmpty -> paidOrdersToString paidOrders
            | _ -> printfn "Nothing found!"

        (newState)
    | HelpRequested ->
        printfn "%s" (createHelpText ())

        (state)
    | NotParsable originalInput ->
        printfn
            """"%s" was not parsable. %s"""
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
