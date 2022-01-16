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
    | Success of PaidCart
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

(*
    Returns a list of train-stations
*)
let private getTrainStations: list<TrainStation> =
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
let private generateTrips =
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
let private maximumTicketAmount = 1

(*
    Returns a list of all possible trips
*)
let private getAllTrips: list<Trip> = generateTrips

let private createTicketForTrip (trip: Trip) (ticketNumber: int) (ticketPrice: float) (ticketType: TicketType) : Ticket =
    { ticketNr = { nr = ticketNumber }
      ticketPrice = { price = ticketPrice }
      ticketType = ticketType
      departure = trip.departure
      arrival = trip.arrival
      trainType = Railjet
      departureTime = DateTime.Now
      arrivalTime = DateTime.Now }

let private genTicketAmount maxValue = System.Random().Next maxValue + 1 //prevent 0

let private adaptPriceToTicketType (ticketType: TicketType) (price: float) =
    match ticketType with
    | AdultTicket -> price * 1.
    | JuniorTicket -> price * 0.5
    | SeniorTicket -> price * 0.8
    | PetTicket -> price * 0.2

let rec private createTicketFromTicketType (types: list<TicketType>) (idx: int) (trip: Trip) : list<Ticket> =
    match types with
    | t :: types ->
        let ticketPriceByTicketType =
            adaptPriceToTicketType t trip.basePrice.price

        createTicketForTrip trip (idx) ticketPriceByTicketType t
        :: createTicketFromTicketType types idx trip
    | [] -> []

let private generateTickets (trip: Trip) (idx: int) : list<Ticket> =
    let randomTicketAmount = genTicketAmount maximumTicketAmount

    let ticketTypes =
        [ AdultTicket
          SeniorTicket
          JuniorTicket
          PetTicket ]

    Seq.init randomTicketAmount (fun _ -> createTicketFromTicketType ticketTypes idx trip)
    |> List.concat

let rec private generateAllTrips
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
let private allTicketMap = generateAllTrips Map.empty getAllTrips 1

// This stores all tickets of the application
let private allTicketsList: list<Ticket> =
    allTicketMap.Values |> Seq.cast |> List.concat

// TODO: watch for exception
let private filterTrips (allTickets: Map<SimpleTrip, list<Ticket>>) (trip: SimpleTrip) : list<Ticket> = allTickets.Item trip

let requestTicket (departure: DepartureTrainStation) (arrival: ArrivalTrainStation) =
    filterTrips allTicketMap { departure = departure.name; arrival = arrival.name }

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
let emptyUnpaidCart () : UnpaidCart = { tickets = [] }
let emptyPaidCart () : PaidCart = { tickets = [] }

// finds a ticket in the given list
let private findTicket (ticketNr: TicketNumber) (ticketType: TicketType) (tickets: list<Ticket>) : Ticket =
    // TODO: watch for exception
    tickets
    |> List.find
        (fun t ->
            t.ticketNr.nr.Equals ticketNr.nr
            && t.ticketType.Equals ticketType)

let private equalsShoppingCartEntry (ticketNr: TicketNumber) (ticketType: TicketType) (item: ShoppingCartEntry) =
    (item.ticket.ticketNr.Equals ticketNr
     && item.ticket.ticketType.Equals ticketType)

let private isTicketAlreadyInCart (cart: UnpaidCart) (ticketNr: TicketNumber) (ticketType: TicketType) =
    cart.tickets
    |> List.filter (equalsShoppingCartEntry ticketNr ticketType)

// returns a new cart without the ticket
let private removeTicketFromShoppingList (tickets: list<ShoppingCartEntry>) (ticketNr: TicketNumber) (ticketType: TicketType) =
    tickets
    |> List.filter (fun item -> not (equalsShoppingCartEntry ticketNr ticketType item))

let addTicketToCart
    (cart: UnpaidCart)
    (ticketNr: TicketNumber)
    (ticketType: TicketType)
    (ticketQuantity: TicketQuantity)
    : UnpaidCart =
    // look for the ticket to add in every
    let ticketToAdd =
        findTicket ticketNr ticketType allTicketsList

    let foundCartItems =
        isTicketAlreadyInCart cart ticketNr ticketType

    let newItem =
        match foundCartItems with
        | x :: _ ->
            { ticket = x.ticket
              quantity = x.quantity + ticketQuantity }
        | [] ->
            { ticket = ticketToAdd
              quantity = ticketQuantity }

    let newCart =
        removeTicketFromShoppingList cart.tickets ticketNr ticketType

    { tickets = newItem :: newCart }

let removeTicketFromCart
    (cart: UnpaidCart)
    (ticketNr: TicketNumber)
    (ticketType: TicketType)
    (ticketQuantity: TicketQuantity)
    : UnpaidCart =
    let newCart =
        removeTicketFromShoppingList cart.tickets ticketNr ticketType

    let newItem =
        cart.tickets
        |> List.filter (equalsShoppingCartEntry ticketNr ticketType)
        |> List.map
            (fun item ->
                { ticket = item.ticket
                  quantity = item.quantity - ticketQuantity })
        |> List.filter (fun item -> item.quantity > 0)

    { tickets = List.append newItem newCart }

let clearCart (cart: UnpaidCart) = emptyUnpaidCart ()

let rec private convertUnpaidItemsToPaidItems
    (unpaidItems: list<ShoppingCartEntry>)
    (paymentMethod: PaymentMethod)
    : list<PaidCartEntry> =
    match unpaidItems with
    | x :: xs ->
        { ticket = x.ticket
          quantity = x.quantity
          orderDate = DateTime.Now
          paymentMethod = paymentMethod }
        :: convertUnpaidItemsToPaidItems xs paymentMethod
    | [] -> []

let private convertUnpaidCartToPaidCart (unpaidCart: UnpaidCart) (paymentMethod: PaymentMethod) : PaidCart =
    { tickets = convertUnpaidItemsToPaidItems unpaidCart.tickets paymentMethod }

let payCart (cart: UnpaidCart) (paymentMethod: PaymentMethod) : PaymentResult =
    let paidCart =
        convertUnpaidCartToPaidCart cart paymentMethod

    Success paidCart

let printSearchTrips (tickets: list<Ticket>) = ""
let printRequestTicket (tickets: list<Ticket>) = ""
let printUnpaidCart (cart: UnpaidCart) = ""
let printPaidCard (cart: PaidCart) = ""

type State = string

let init () : State = ""

type CartManipulation = {ticketNr: TicketNumber; ticketType: TicketType; ticketQuantity: TicketQuantity}

type Message =
    | PrintTrainStations
    | SearchTrips of SimpleTrip
    | RequestTicket of SimpleTrip
    | AddTicketToCart of CartManipulation
    | PrintCart
    | RemoveTicketFromCart of CartManipulation
    | ClearCart
    | PayCart of PaymentMethod
    | ShowPaidOrders

let update (msg: Message) (model: State) : State =
    match msg with
    | PrintTrainStations -> "A"
//    | SearchTrips x -> printSearchTrips |> searchTrips x.departure x.arrival
//    | RequestTicket x -> printRequestTicket requestTicket x.departure x.arrival
//    | AddTicketToCart x -> printUnpaidCart (addTicketToCart model.unpaidCart x.ticketNr x.ticketType x.ticketQuantity)
    | PrintCart -> "A"
//    | RemoveTicketFromCart x -> printUnpaidCart (removeTicketFromCart model.unpaidCart x.ticketNr x.ticketType x.ticketQuantity)
//    | ClearCart -> printUnpaidCart (clearCart model.unpaidCart)
//    | PayCart x -> printPaidCard (payCart model.unpaidCart x)
    | ShowPaidOrders -> "A"
