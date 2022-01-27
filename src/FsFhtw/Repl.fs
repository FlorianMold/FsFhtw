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

let printTrainStations (trainStations: list<Domain.TrainStation>) =
    printfn "------------------------------"

    for trainStation in trainStations do
        printfn "     %s " trainStation.name

    printfn "------------------------------"

let printSearchTrips (tickets: list<Domain.Ticket>) =
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


let printRequestTicket (tickets: list<Domain.Ticket>) =
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

let printTickets (shoppingCartEntry: list<ShoppingCartEntry>) =
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
    printfn $"Total: %.2f{computeTotalUnpaidCart {tickets = shoppingCartEntry}} $"

let printUnpaidCart (newCart: Domain.UnpaidCart) =
    match newCart.tickets with
    | [] -> printfn "Cart is empty!"
    | _ -> printTickets newCart.tickets

let printPaidCartItems paidCart =
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

let printPaidCart (paymentResult: Domain.PaymentResult) =
    printfn "Your Invoice:"
    match paymentResult with
    | Success x ->
        printfn "| Ticket-nr |    From     |     To      |   Price   |      Departuretime - Arrivaltime      |    Train Type   |   Ticket Type   | Quantity |     Order Date     |  Payment Method  |"

        printfn "------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------"

        printPaidCartItems x

        printfn "------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------"

        printfn $"Total: %.2f{computeTotalPaidCart x} $"

    | Bounce x -> printfn "%O" x

let printPaidOrders (paidCarts: list<PaidCart>) =

    for paidCart in paidCarts do
        printfn "| Ticket-nr |    From     |     To      |   Price   |      Departuretime - Arrivaltime      |    Train Type   |   Ticket Type   | Quantity |     Order Date     |  Payment Method  |"
        printfn "------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------"
        printPaidCartItems paidCart
        printfn "------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------"
        printfn $"Total: %.2f{computeTotalPaidCart paidCart} $"
        printfn "------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------"

let printAvailableTrips (trips: list<Trip>) =
    printfn "No trips found. Available trips are:"
    trips |> List.iter (fun item -> printfn $"%s{item.arrival.name} - %s{item.departure.name}" )

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
                printUnpaidCart cart

        let message =
            match msg with
            | Domain.PrintTrainStations -> printTrainStations stations
            | Domain.SearchTrips _ when not ticketList.IsEmpty -> printSearchTrips ticketList
            | Domain.RequestTicket _ when not ticketList.IsEmpty -> printRequestTicket ticketList
            | Domain.AddTicketToCart _ -> printMessageIfCartHasNotChanged "Nothing was added to the cart!"
            | Domain.PrintCart -> printUnpaidCart cart
            | Domain.RemoveTicketFromCart _ -> printMessageIfCartHasNotChanged "Nothing was removed to the cart!"
            | Domain.ClearCart -> printUnpaidCart cart
            | Domain.PayCart _ -> printPaidCart paymentResult
            | Domain.ShowPaidOrders -> if paidOrders.IsEmpty then printfn "No paid orders!" else printPaidOrders paidOrders
            | _ -> printAvailableTrips Domain.getAllTrips

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
