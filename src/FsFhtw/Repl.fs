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
// TODO(Aytac):neuen cart vergleichen muesen, damit wir feststellen koennen, dass sich nicht veraendert hat.
// TODO(Aytac): e.g.: addTocart mit nicht exister ticket-id...
let trainStationsToString (trainStation: list<Domain.TrainStation>) = "Train stations"
let searchTripsToString (tickets: list<Domain.Ticket>) = "Search trips"
let requestTicketToString (tickets: list<Domain.Ticket>) = "Request tickets"
let unpaidCartToString (oldCard: Domain.UnpaidCart) (newCart: Domain.UnpaidCart) = "Unpaid Cart"
let paidCartToString (cart: Domain.PaymentResult) = "paid cart to string"
let paidOrdersToString (paidCarts: list<PaidCart>) = "paid order"

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
