module Domain

open System

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

type TicketQuantity = int

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

type ShoppingCartEntry = { ticket: Ticket; quantity: int }

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

type GetTrainStations = list<TrainStation>

type RequestTicket = TicketNumber -> list<Ticket>
type PrintRequestTicket = list<Ticket> -> unit

type AddTicketToCart = UnpaidCart -> TicketNumber -> TicketQuantity -> TicketType -> UnpaidCart
type PrintAddTicketToCart = UnpaidCart -> Ticket -> unit

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

// TODO: temporary lock this value
let maximumTicketAmount = 1

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
let generateTickets (trip: Trip) (idx: int) : list<Ticket> =
    let randomTicketAmount = genTicketAmount maximumTicketAmount

    let ticketTypes =
        [ AdultTicket
          SeniorTicket
          JuniorTicket
          PetTicket ]

    Seq.init randomTicketAmount (fun _ -> createTicketFromTicketType ticketTypes idx trip)
    |> List.concat

// TODO: watch for exception
let filterTrips (allTickets: Map<SimpleTrip, list<Ticket>>) (trip: SimpleTrip) : list<Ticket> = allTickets.Item trip

let rec generateAllTrips
    (map: Map<SimpleTrip, list<Ticket>>)
    (trips: list<Trip>)
    (idx: int)
    : Map<SimpleTrip, list<Ticket>> =
    match trips with
    | head :: tail ->
        generateAllTrips
            (map.Add(
                { departure = head.departure.name
                  arrival = head.arrival.name },
                generateTickets head idx
            ))
            tail
            (idx + 1)
    | [] -> map

// This stores all tickets in the application, grouped-by trip
let allTicketMap = generateAllTrips Map.empty getAllTrips 1

// This stores all tickets of the application
let allTicketsList: list<Ticket> =
    allTicketMap.Values |> Seq.cast |> List.concat

let filteredTrips = filterTrips allTicketMap

let requestTicket (departure: DepartureTrainStation) (arrival: ArrivalTrainStation) =
    filteredTrips
        { departure = departure.name
          arrival = arrival.name }

let searchTrips (departure: DepartureTrainStation) (arrival: ArrivalTrainStation) =
    let filteredTickets = requestTicket departure arrival

    // group the elements by ticket id and take the first element of every list, because the ticket is always the same
    filteredTickets
    |> List.groupBy (fun (ticket: Ticket) -> ticket.ticketNr.nr)
    // function -> https://stackoverflow.com/questions/37508934/difference-between-let-fun-and-function-in-f/37509083
    |> List.choose
        (function
        | _, x :: _ -> Some x
        | _ -> None)

// Creates an empty cart
let emptyCart () : UnpaidCart = { tickets = [] }

let cart = { tickets = [] }

// finds a ticket in the given list
let findTicket (ticketNr: TicketNumber) (ticketType: TicketType) (tickets: list<Ticket>) : Ticket =
    // TODO: watch for exception
    tickets
    |> List.find
        (fun t ->
            t.ticketNr.nr.Equals ticketNr.nr
            && t.ticketType.Equals ticketType)

// TODO: check if item is already in the cart, and sum the quantities, otherwise add the element
let addTicketToCart
    (cart: UnpaidCart)
    (ticketNr: TicketNumber)
    (ticketType: TicketType)
    (ticketQuantity: TicketQuantity)
    : UnpaidCart =
    let ticketToAdd =
        findTicket ticketNr ticketType allTicketsList

    let entry =
        { ticket = ticketToAdd
          quantity = ticketQuantity }

    { tickets = entry :: cart.tickets }

let removeTicketFromCart
    (cart: UnpaidCart)
    (ticketNr: TicketNumber)
    (ticketType: TicketType)
    (ticketQuantity: TicketQuantity)
    : UnpaidCart =
    let filter (item: ShoppingCartEntry) =
        (item.ticket.ticketNr.Equals ticketNr
         && item.ticket.ticketType.Equals ticketType)

    let newCart =
        cart.tickets
        |> List.filter (fun item -> not (filter item))

    let newItem =
        cart.tickets
        |> List.filter filter
        |> List.map
            (fun item ->
                { ticket = item.ticket
                  quantity = item.quantity - ticketQuantity })
        |> List.filter (fun item -> item.quantity > 0)

    { tickets = List.append newItem newCart }
