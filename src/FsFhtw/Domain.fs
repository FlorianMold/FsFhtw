module Domain

open System

// AREA :Types
type TrainStation = { name: string }

type DepartureTrainStation = TrainStation

type ArrivalTrainStation = TrainStation


type Price = { price: float }
type TicketPrice = Price
type TripPrice = Price

type Trip =
    { departure: DepartureTrainStation
      arrival: ArrivalTrainStation
      basePrice: TripPrice }

type SimpleTrip = { departure: string; arrival: string }

type TrainType =
    | Railjet
    | Nightjet
    | Eurocity
    | Intercity
    | Cityjet

type TicketNumber = int

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

type UnpaidCart = { tickets: list<ShoppingCartEntry> }

type PaidCart = { tickets: list<PaidCartEntry> }

type CartManipulation =
    { ticketNr: TicketNumber
      ticketType: TicketType
      ticketQuantity: TicketQuantity }

type ShoppingCart =
    | PaidCart of PaidCart
    | UnpaidCart of UnpaidCart

type BounceReason =
    | NoItemsInCart
    | PaymentProcessorFailed
    | CustomerWentOutOfMoney
    | CustomerCanceled

type PaymentResult =
    | Success of PaidCart
    | Bounce of BounceReason

// Function types

type SearchTrips = SimpleTrip -> list<Ticket>

type RequestTicket = SimpleTrip -> list<Ticket>

type AddTicketToCart = UnpaidCart -> CartManipulation -> UnpaidCart

type RemoveTicketFromCart = UnpaidCart -> CartManipulation -> UnpaidCart

type ClearCart = UnpaidCart -> UnpaidCart

type PayCart = UnpaidCart -> PaymentMethod -> PaymentResult

type State = UnpaidCart * list<Ticket> * list<TrainStation> * PaymentResult * list<PaidCart>

// AREA: Utility

// TODO: Take the next day if hour > 24
let setTime h m (t: DateTime) =
    match h with
    | h when h > 23 -> DateTime(t.Year, t.Month, t.Day + 1, h % 24, m, 0)
    | _ -> DateTime(t.Year, t.Month, t.Day, h, m, 0)

// AREA: PRIVATE API

let private maximumTicketAmount = 10
let private minimumTicketAmount = 5

let private maximumTicketPrice = 20
let private minimumTicketPrice = 10

/// Generates a random number between minimumValue and (maxValue)
let private randomNumber maxValue minimumValue =
    (System.Random().Next(maxValue - minimumValue))
    + minimumValue
    + 1 //prevent 0

/// A list of all possible train-stations.
let private allTrainStations: list<TrainStation> =
    [ { name = "Vienna" }
      { name = "Linz" }
      { name = "Klagenfurt" }
      { name = "Bregenz" }
      { name = "Salzburg" }
      { name = "Innsbruck" }
      { name = "Graz" }
      { name = "Eisenstadt" } ]

/// Naive random trip generation
let rec private generateTrips (stations: list<TrainStation>) : list<Trip> =
    match stations with
    | x :: y :: xs ->
        { departure = { name = x.name }
          arrival = { name = y.name }
          basePrice = { price = randomNumber maximumTicketPrice minimumTicketPrice } }
        :: generateTrips xs
    | _ -> []

/// Returns a list of all possible trips.
let private getAllTrips: list<Trip> =
    List.append (generateTrips allTrainStations) (generateTrips (List.rev allTrainStations))

/// Generates a ticket for the trip with the ticket-nr, price and type.
let private createTicketForTrip
    (trip: Trip)
    (ticketNumber: int)
    (ticketPrice: float)
    (ticketType: TicketType)
    (departure: DateTime)
    : Ticket =
    { ticketNr = ticketNumber
      ticketPrice = { price = ticketPrice }
      ticketType = ticketType
      departure = trip.departure
      arrival = trip.arrival
      trainType = Railjet
      departureTime = departure
      arrivalTime = setTime (departure.Hour + 1) 0 departure }

/// Adapts the base price of a ticket to the prices for adults, juniors and seniors.
let private adaptPriceToTicketType (ticketType: TicketType) (price: float) =
    match ticketType with
    | AdultTicket -> price * 1.
    | JuniorTicket -> price * 0.5
    | SeniorTicket -> price * 0.8
    | PetTicket -> price * 0.2

/// Generates a ticket for every ticket-type with the ticket-nr and the trip
let rec private createTicketFromTicketType
    (types: list<TicketType>)
    (ticketNumber: int)
    (departure: DateTime)
    (trip: Trip)
    : list<Ticket> =
    match types with
    | t :: types ->
        // Calculate price of ticket
        let ticketPriceByTicketType =
            adaptPriceToTicketType t trip.basePrice.price

        createTicketForTrip trip ticketNumber ticketPriceByTicketType t departure
        :: createTicketFromTicketType types ticketNumber departure trip
    | [] -> []

/// Generates a ticket for every ticket-type with the given ticket-nr
let private generateTickets (trip: Trip) (ticketNumber: int) : list<Ticket> =
    let randomTicketAmount =
        randomNumber maximumTicketAmount minimumTicketAmount

    let now = DateTime.Now
    let nextHour = setTime (now.Hour + 1) 0 now

    let ticketTypes =
        [ AdultTicket
          SeniorTicket
          JuniorTicket
          PetTicket ]

    Seq.init
        randomTicketAmount
        (fun idx ->
            let firstDeparture = setTime (nextHour.Hour + idx) 0 nextHour
            createTicketFromTicketType ticketTypes (ticketNumber + idx) firstDeparture trip)
    |> List.concat

/// Generates all trips with their tickets for the application.
let rec private generateAllTrips
    (map: Map<SimpleTrip, list<Ticket>>)
    (trips: list<Trip>)
    (ticketNumber: int)
    : Map<SimpleTrip, list<Ticket>> =
    match trips with
    | head :: tail ->
        generateAllTrips
            (map.Add(
                { departure = head.departure.name
                  arrival = head.arrival.name },
                generateTickets head (ticketNumber * 100)
            ))
            tail
            (ticketNumber + 1)
    | [] -> map

// Stores all tickets in the application, grouped-by trip
let private allTicketMap = generateAllTrips Map.empty getAllTrips 1

// This stores all tickets of the application.
let private allTicketsList: list<Ticket> =
    allTicketMap.Values |> Seq.cast |> List.concat

/// Find tickets for the given trip.
let private filterTrips (allTickets: Map<SimpleTrip, list<Ticket>>) (trip: SimpleTrip) : list<Ticket> =
    if allTickets.ContainsKey trip then
        allTickets.Item trip
    else
        []

/// Looks for the given ticket-number and ticket-type in ticket-list
let private findTicket (ticketNr: TicketNumber) (ticketType: TicketType) (tickets: list<Ticket>) : list<Ticket> =
    tickets
    |> List.filter
        (fun t ->
            t.ticketNr.Equals ticketNr
            && t.ticketType.Equals ticketType)


/// Is the ticket-number and ticket-type the same as in the shopping-cart-entry.
/// Comparison with ticket-number and ticket-type.
let private equalsShoppingCartEntry (ticketNr: TicketNumber) (ticketType: TicketType) (item: ShoppingCartEntry) =
    (item.ticket.ticketNr.Equals ticketNr
     && item.ticket.ticketType.Equals ticketType)

/// Checks if the ticket with the ticket-number and ticket-type is already in the unpaid cart.
let private isTicketAlreadyInCart (cart: UnpaidCart) (ticketNr: TicketNumber) (ticketType: TicketType) =
    cart.tickets
    |> List.filter (equalsShoppingCartEntry ticketNr ticketType)

/// Removes the ticket with the ticket-number and ticket-type from the list.
let private removeTicketFromShoppingList
    (tickets: list<ShoppingCartEntry>)
    (ticketNr: TicketNumber)
    (ticketType: TicketType)
    =
    tickets
    |> List.filter (fun item -> not (equalsShoppingCartEntry ticketNr ticketType item))


/// Adds the ticket to the cart with the quantity.
/// Checks if the ticket is already in the cart, increases the quantity, if true
/// Otherwise add the item to the cart.
let private addTicketToCart' (cart: UnpaidCart) (ticket: Ticket) (ticketQuantity: TicketQuantity) : UnpaidCart =
    // Is the item already in the cart?
    let foundCartItems =
        isTicketAlreadyInCart cart ticket.ticketNr ticket.ticketType

    let newItem =
        match foundCartItems with
        | x :: _ ->
            { ticket = x.ticket
              quantity = x.quantity + ticketQuantity }
        | [] ->
            { ticket = ticket
              quantity = ticketQuantity }

    /// Remove the item, if it is already in the cart
    let newCart =
        removeTicketFromShoppingList cart.tickets ticket.ticketNr ticket.ticketType

    { tickets = newItem :: newCart }

// Converts the given list of shopping-cart entries to a list of paid-entries.
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

/// Converts the unpaid-cart to a paid cart with the given payment-method.
let private convertUnpaidCartToPaidCart (unpaidCart: UnpaidCart) (paymentMethod: PaymentMethod) : PaidCart =
    { tickets = convertUnpaidItemsToPaidItems unpaidCart.tickets paymentMethod }

// AREA: PUBLIC API

// Creates an empty cart
let private emptyUnpaidCart () : UnpaidCart = { tickets = [] }

/// Find all tickets (includes the type of tickets) for the given departure and arrival.
let private requestTicket (simpleTrip: SimpleTrip) : list<Ticket> = filterTrips allTicketMap simpleTrip

/// Find all trips for the given departure and arrival.
let private searchTrips (simpleTrip: SimpleTrip) : list<Ticket> =
    let filteredTickets = requestTicket simpleTrip

    // Group the elements by ticket id and take the first element of every list,
    // because the ticket is always the same and the only difference is the ticket-type
    filteredTickets
    |> List.groupBy (fun (ticket: Ticket) -> ticket.ticketNr)
    // function -> https://stackoverflow.com/questions/37508934/difference-between-let-fun-and-function-in-f/37509083
    |> List.choose
        (function
        | _, x :: _ -> Some x
        | _ -> None)

/// Adds the ticket with the given ticket-number and ticket-type with the quantity to the cart.
/// If the ticket doesn't exist, the cart stays the same.
let private addTicketToCart (cart: UnpaidCart) (cartManipulationOp: CartManipulation) : UnpaidCart =
    // Look for the ticket to add in every.
    let ticketToAdd =
        findTicket cartManipulationOp.ticketNr cartManipulationOp.ticketType allTicketsList

    match ticketToAdd with
    | [ ticket ] -> addTicketToCart' cart ticket cartManipulationOp.ticketQuantity
    | _ -> cart

/// Removes the ticket with the ticket-number and type with the quantity from the cart.
/// If the quantity becomes zero, the item is removed from the cart.
let private removeTicketFromCart
    (cart: UnpaidCart)
    (cartManipulationOp: CartManipulation)

    : UnpaidCart =
    let newCart =
        removeTicketFromShoppingList cart.tickets cartManipulationOp.ticketNr cartManipulationOp.ticketType

    let newItem =
        cart.tickets
        |> List.filter (equalsShoppingCartEntry cartManipulationOp.ticketNr cartManipulationOp.ticketType)
        // reduce quantity of found item
        |> List.map
            (fun item ->
                { ticket = item.ticket
                  quantity = item.quantity - cartManipulationOp.ticketQuantity })
        // quantity < 0 is removed
        |> List.filter (fun item -> item.quantity > 0)

    // Append the new ticket to the list.
    { tickets = List.append newItem newCart }

/// Returns a new empty cart.
let private clearCart (cart: UnpaidCart) = emptyUnpaidCart ()

/// Pays the given cart with the payment-method.
let private payCart (cart: UnpaidCart) (paymentMethod: PaymentMethod) : PaymentResult =

    if List.isEmpty cart.tickets then
        Bounce NoItemsInCart
    else
        let paidCart =
            convertUnpaidCartToPaidCart cart paymentMethod

        Success paidCart

let rec private compareShoppingCartLists (first: list<ShoppingCartEntry>) (second: list<ShoppingCartEntry>) =
    match first, second with
    | [], [] -> true
    | x :: xs, y :: ys ->
        x.quantity = y.quantity
        && compareShoppingCartLists xs ys
    | _ -> false

/// Compares two shopping-carts
let compareCarts (firstCart: UnpaidCart) (secondCart: UnpaidCart) =
    if firstCart.tickets.Length <> secondCart.tickets.Length then
        false
    else
        compareShoppingCartLists firstCart.tickets secondCart.tickets


type TicketShopApi =
    { requestTicket: RequestTicket
      searchTrips: SearchTrips
      addTicketToCart: AddTicketToCart
      removeTicketFromCart: RemoveTicketFromCart
      clearCart: ClearCart
      payCart: PayCart }

let ticketShopApi: TicketShopApi =
    { requestTicket = requestTicket
      searchTrips = searchTrips
      addTicketToCart = addTicketToCart
      removeTicketFromCart = removeTicketFromCart
      clearCart = clearCart
      payCart = payCart }

let init () : State =
    (emptyUnpaidCart (), [], allTrainStations, Success { tickets = [] }, [])

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
    let (cart, ticketList, stations, paymentResult, paidOrders) = model

    match msg with
    | PrintTrainStations -> (cart, ticketList, allTrainStations, paymentResult, paidOrders)
    | SearchTrips trip -> (cart, searchTrips trip, stations, paymentResult, paidOrders)
    | RequestTicket trip -> (cart, requestTicket trip, stations, paymentResult, paidOrders)
    | AddTicketToCart cmo -> ((addTicketToCart cart cmo), ticketList, stations, paymentResult, paidOrders)
    | PrintCart -> model
    | RemoveTicketFromCart cmo -> ((removeTicketFromCart cart cmo), ticketList, stations, paymentResult, paidOrders)
    | ClearCart -> ((clearCart cart), ticketList, stations, paymentResult, paidOrders)
    | PayCart x ->
        let paymentResult = payCart cart x

        // Add the paid-cart to the paid-orders
        let paidCart =
            match paymentResult with
            | Success x -> [ x ]
            | _ -> []

        (emptyUnpaidCart (), ticketList, stations, paymentResult, List.append paidCart paidOrders)
    | ShowPaidOrders -> model
