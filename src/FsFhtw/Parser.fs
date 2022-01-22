module Parser

open System
open Domain

let safeEquals (it: string) (theOther: string) =
    String.Equals(it, theOther, StringComparison.OrdinalIgnoreCase)

[<Literal>]
let HelpLabel = "Help"

type Command =
    | PrintTrainStations
    | SearchTrips of SimpleTrip
    | RequestTicket of SimpleTrip
    | AddTicketToCart of CartManipulation
    | RemoveTicketFromCart of CartManipulation
    | PrintCart
    | ClearCart
    | PayCart of PaymentMethod
    | ShowPaidOrders
    | Help
    | ParseFailed

/// Construct a pay-cart
let tryParsePayCartOperation (paymentMethodArg: string) valueConstructor =
    match paymentMethodArg with
    | "VISA" when safeEquals paymentMethodArg (nameof PaymentMethod.VISA) -> valueConstructor VISA
    | "PayPal" when safeEquals paymentMethodArg (nameof PaymentMethod.PayPal) -> valueConstructor PayPal
    | "Klarna" when safeEquals paymentMethodArg (nameof PaymentMethod.Klarna) -> valueConstructor Klarna
    | _ -> ParseFailed

let tryParseSimpleTripsOperation (departureArg: string) (arrivalArg: string) valueConstructor =
    valueConstructor
        { departure = departureArg
          arrival = arrivalArg }

type TicketParseResult =
    | SuccessParse of TicketType
    | FailedParse

let tryParseCartManipulationOperation
    (ticketNrArg: string)
    (ticketTypeArg: string)
    (ticketQuantityArg: string)
    valueConstructor
    =
    let (ticketNrWorked, ticketNrArg') = Int32.TryParse ticketNrArg
    let (ticketQuantityWorked, ticketQuantityArg') = Int32.TryParse ticketQuantityArg

    let ticketType =
        match ticketTypeArg with
        | "AdultTicket" when safeEquals ticketTypeArg (nameof TicketType.AdultTicket) -> SuccessParse AdultTicket
        | "JuniorTicket" when safeEquals ticketTypeArg (nameof TicketType.JuniorTicket) -> SuccessParse JuniorTicket
        | "SeniorTicket" when safeEquals ticketTypeArg (nameof TicketType.SeniorTicket) -> SuccessParse SeniorTicket
        | "PetTicket" when safeEquals ticketTypeArg (nameof TicketType.PetTicket) -> SuccessParse PetTicket
        | _ -> FailedParse

    match ticketType with
    | SuccessParse ticketType when ticketNrWorked && ticketQuantityWorked ->
        valueConstructor
            { ticketNr = ticketNrArg'
              ticketType = ticketType
              ticketQuantity = ticketQuantityArg' }
    | _ -> ParseFailed

let parseInput (input: string) =
    let parts = input.Split(' ') |> List.ofArray

    match parts with
    | [ verb ] when safeEquals verb (nameof Domain.PrintTrainStations) -> PrintTrainStations

    | [ verb; departure; arrival ] when safeEquals verb (nameof Domain.SearchTrips) ->
        tryParseSimpleTripsOperation departure arrival (SearchTrips)

    | [ verb; departure; arrival ] when safeEquals verb (nameof Domain.RequestTicket) ->
        tryParseSimpleTripsOperation departure arrival (RequestTicket)

    | [ verb; ticketNr; ticketType; amount ] when safeEquals verb (nameof Domain.AddTicketToCart) ->
        tryParseCartManipulationOperation ticketNr ticketType amount (AddTicketToCart)

    | [ verb ] when safeEquals verb (nameof Domain.PrintCart) -> PrintCart

    | [ verb; ticketNr; ticketType; amount ] when safeEquals verb (nameof Domain.RemoveTicketFromCart) ->
        tryParseCartManipulationOperation ticketNr ticketType amount (RemoveTicketFromCart)

    | [ verb ] when safeEquals verb (nameof Domain.ClearCart) -> ClearCart

    | [ verb ] when safeEquals verb (nameof Domain.ShowPaidOrders) -> ShowPaidOrders

    | [ verb; paymentMethod ] when safeEquals verb (nameof Domain.PayCart) -> tryParsePayCartOperation paymentMethod (PayCart)

    | [ verb ] when safeEquals verb HelpLabel -> Help
    | _ -> ParseFailed
