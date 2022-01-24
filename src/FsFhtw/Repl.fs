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

let trainStationsToString (trainStations: list<Domain.TrainStation>) = 
    for trainStation in trainStations do printf "%s, " trainStation.name;
    ""

let searchTripsToString (tickets: list<Domain.Ticket>) = 
    printfn "| ticket-nr |   from   |   to   |      Departuretime - Arrivaltime      |   trainType   |"
    printfn "-----------------------------------------------------------------------------------------"
    for ticket in tickets 
        do printfn "|    %d    |  %s  |  %s  | %s  -  %s | %O |" 
            ticket.ticketNr 
            ticket.departure.name 
            ticket.arrival.name 
            (ticket.departureTime.ToString "dd.mm.yyyy hh:mm") 
            (ticket.arrivalTime.ToString "dd.mm.yyyy hh:mm")  
            ticket.trainType;
    "-----------------------------------------------------------------------------------------";

let requestTicketToString (tickets: list<Domain.Ticket>) = 
    printfn "| ticket-nr |   from   |   to   |      Departuretime - Arrivaltime      |     trainType    |     ticketType    |"
    printfn "----------------------------------------------------------------------------------------------------------------"
    for ticket in tickets 
        do printfn "|    %d    |  %s  |  %s  | %s  -  %s | %O | %O |" 
            ticket.ticketNr 
            ticket.departure.name 
            ticket.arrival.name 
            (ticket.departureTime.ToString "dd.mm.yyyy hh:mm") 
            (ticket.arrivalTime.ToString "dd.mm.yyyy hh:mm")  
            ticket.trainType
            ticket.ticketType;
    "----------------------------------------------------------------------------------------------------------------"

let unpaidCartToString (oldCart: Domain.UnpaidCart) (newCart: Domain.UnpaidCart) = 
   if oldCart.tickets.Length = newCart.tickets.Length 
    then 
        "for oldCart"
    else 
        printfn "| ticket-nr |   from   |   to   |      Departuretime - Arrivaltime      |    trainType   |    ticketType    | quantity |"
        printfn "-------------------------------------------------------------------------------------------------------------------------"
        for ticket in newCart.tickets 
            do printfn "|    %d    |  %s  |  %s  | %s  -  %s |     %O    |   %O   |     %d    |" 
                ticket.ticket.ticketNr
                ticket.ticket.departure.name 
                ticket.ticket.arrival.name 
                (ticket.ticket.departureTime.ToString "dd.mm.yyyy hh:mm") 
                (ticket.ticket.arrivalTime.ToString "dd.mm.yyyy hh:mm")  
                ticket.ticket.trainType
                ticket.ticket.ticketType
                ticket.quantity;
        "-------------------------------------------------------------------------------------------------------------------------";

let paidCartToString (cart: Domain.PaymentResult) = 
    "paid cart to string"
let paidOrdersToString (paidCarts: list<PaidCart>) = 
    ""

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

        (newState, message)
    | HelpRequested ->
        let message = createHelpText ()
        (state, message)
    | NotParsable originalInput ->
        let message =
            sprintf
                """"%s" was not parsable. %s"""
                originalInput
                "You can get information about known commands by typing \"Help\""

        (state, message)

let print (state: State, outputToPrint: string) =
    printfn "%s\n" outputToPrint
    printf "> "

    state

let rec loop (state: State) =
    Console.ReadLine()
    |> read
    |> evaluate Domain.update state
    |> print
    |> loop
