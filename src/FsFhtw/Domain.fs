module Domain

open System
open System.Collections.Generic

//type AustrianMainTrainStation = {Vienna | Linz | Graz | Eisenstadt | Bregenz | Salzburg | Innsbruck | Klagenfurt | St_Poelten}
type AustrianMainTrainStation = { name: string }

type TrainStation = AustrianMainTrainStation

type DepartureTrainStation = TrainStation

type ArrivalTrainStation = TrainStation

type Price = { price: float }
type TicketPrice = Price
type TripPrice = Price

type Trip =
    { departure: DepartureTrainStation
      arrival: ArrivalTrainStation
      basePrice: TripPrice }

// TODO: temp
type SimpleTrip = { departure: string; arrival: string }

type TrainType =
    | Railjet
    | Nightjet
    | Eurocity
    | Intercity
    | Cityjet

type TicketNumber = { nr: int }

type TicketType =
    | SeniorTicket
    | JuniorTicket
    | AdultTicket
    | PetTicket

type TicketQuantity = { quantity: int }

type TimeStamp = DateTime
type DepartureTime = TimeStamp
type ArrivalTime = TimeStamp

type PaymentMethod =
    | VISA
    | PayPal
    | Klarna

type Ticket =
    { ticketNr: TicketNumber
      ticketPrice: Price
      ticketType: TicketType
      departure: DepartureTrainStation
      arrival: ArrivalTrainStation
      trainType: TrainType
      departureTime: DepartureTime
      arrivalTime: ArrivalTime }

type ShoppingCartEntry =
    { ticket: Ticket
      quantity: TicketQuantity }

type PaidCartEntry =
    { ticket: Ticket
      quantity: TicketQuantity
      orderDate: DateTime
      paymentMethod: PaymentMethod }

type EmptyCart = UnpaidCart

type UnpaidCart = { tickets: list<ShoppingCartEntry> }

type PaidCart = { tickets: list<PaidCartEntry> }

type ShoppingCart =
    | PaidCart of PaidCart
    | UnpaidCart of UnpaidCart

type BounceReason =
    | PaymentProcessorFailed
    | CustomerWentOutOfMoney
    | CustomerCanceled

type PaymentResult =
    | Success
    | Bounce of BounceReason

type Orders = list<PaidCart>

// Function types

type PrintStations = list<TrainStation> -> unit

type SearchTrips = ArrivalTrainStation -> DepartureTrainStation -> list<Ticket>
type PrintSearchTrips = list<Ticket> -> unit

type RequestTicket = TicketNumber -> list<Ticket>
type PrintRequestTicket = list<Ticket> -> unit

type AddToCart = TicketNumber -> TicketQuantity -> TicketType -> UnpaidCart
type PrintAddToCart = UnpaidCart -> Ticket -> unit

type PrintCart = UnpaidCart -> unit

type RemoveFromCart = UnpaidCart -> TicketNumber -> TicketType -> TicketQuantity -> UnpaidCart

type ClearCart = UnpaidCart -> EmptyCart

type PayCart = UnpaidCart -> PaymentMethod -> PaymentResult

type ConvertCart = UnpaidCart -> PaidCart

type StorePaidCart = Orders -> PaidCart -> Orders

type ShowPaidOrders = Orders -> unit

type State = int

type Message =
    | Increment
    | Decrement
    | IncrementBy of int
    | DecrementBy of int

let init () : State = 0

let update (msg: Message) (model: State) : State =
    match msg with
    | Increment -> model + 1
    | Decrement -> model - 1
    | IncrementBy x -> model + x
    | DecrementBy x -> model - x

(*
    Returns a list of train-stations
*)
let getTrainStations: list<TrainStation> =
    [ { name = "Vienna" }
      { name = "Linz" }
      { name = "Klagenfurt" }
      { name = "Bregenz" }
      { name = "Salzburg" }
      { name = "St_Poelten" }
      { name = "Innsbruck" }
      { name = "Graz" }
      { name = "Eisenstadt" } ]

// TODO: generate random trips
let generateTrips =
    [ { departure = { name = "Vienna" }
        arrival = { name = "Linz" }
        basePrice = { price = 10 } }
      { departure = { name = "Linz" }
        arrival = { name = "Vienna" }
        basePrice = { price = 12 } }
      { departure = { name = "Klagenfurt" }
        arrival = { name = "Bregenz" }
        basePrice = { price = 12 } } ]

let maximumTicketAmount = 5

(*
    Returns a list of all possible trips
*)
let getAllTrips: list<Trip> = generateTrips

let createTicketForTrip (trip: Trip) (ticketNumber: int) (ticketPrice: float) (ticketType: TicketType) : Ticket =
    { ticketNr = { nr = ticketNumber }
      ticketPrice = { price = ticketPrice }
      ticketType = ticketType
      departure = trip.departure
      arrival = trip.arrival
      trainType = Railjet
      departureTime = DateTime.Now
      arrivalTime = DateTime.Now }

let genTicketAmount maxValue = System.Random().Next maxValue + 1 //prevent 0

let adaptPriceToTicketType (ticketType: TicketType) (price: float) =
    // TODO: move to constants
    match ticketType with
    | AdultTicket -> price * 1.
    | JuniorTicket -> price * 0.5
    | SeniorTicket -> price * 0.8
    | PetTicket -> price * 0.2

let rec createTicketFromTicketType (types: list<TicketType>) (idx: int) (trip: Trip) : list<Ticket> =
    match types with
    | t :: types ->
        let ticketPriceByTicketType =
            adaptPriceToTicketType t trip.basePrice.price

        createTicketForTrip trip (idx) ticketPriceByTicketType t
        :: createTicketFromTicketType types idx trip
    | [] -> []

// TODO: generate random tickets
let generateTickets (trip: Trip) : list<Ticket> =
    let randomTicketAmount = genTicketAmount maximumTicketAmount

    let ticketTypes =
        [ AdultTicket
          SeniorTicket
          JuniorTicket
          PetTicket ]

    Seq.init randomTicketAmount (fun idx -> createTicketFromTicketType ticketTypes (idx + 1) trip)
    |> List.concat

let filterTrips (allTickets: Map<SimpleTrip, list<Ticket>>) (trip: SimpleTrip) : list<Ticket> = allTickets.Item trip

let rec generateAllTrips (map: Map<SimpleTrip, list<Ticket>>) (trips: list<Trip>) : Map<SimpleTrip, list<Ticket>> =
    match trips with
    | head :: tail ->
        generateAllTrips
            (map.Add(
                { departure = head.departure.name
                  arrival = head.arrival.name },
                generateTickets head
            ))
            tail
    | [] -> map

// This stores all trips in the application
let allTickets = generateAllTrips Map.empty getAllTrips

let searchTrips (departure: DepartureTrainStation) (arrival: ArrivalTrainStation) =
    let filteredTrips =
        filterTrips
            allTickets
            { departure = departure.name
              arrival = arrival.name }

    filteredTrips
    |> List.groupBy (fun (ticket: Ticket) -> ticket.ticketNr.nr)
    // function -> https://stackoverflow.com/questions/37508934/difference-between-let-fun-and-function-in-f/37509083
    |> List.choose
        (function
        | _, x :: _ -> Some x
        | _ -> None)
