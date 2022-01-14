module Domain

open System

type AustrianMainTrainStation = Vienna | Linz | Graz | Eisenstadt | Bregenz | Salzburg | Innsbruck | Klagenfurt | St_Poelten

type TrainStation = AustrianMainTrainStation

type DepartureTrainStation = TrainStation

type ArrivalTrainStation = TrainStation

type TrainType = Railjet | Nightjet | Eurocity | Intercity | Cityjet

type TicketNumber = { nr: int }

type TicketType = SeniorTicket | JuniorTicket | AdultTicket | PetTicket

type TicketQuantity = { quantity: int }

type TicketPrice = { price: float }

type TimeStamp = { timestamp: DateTime }
type DepartureTime = TimeStamp
type ArrivalTime = TimeStamp

type PaymentMethod = VISA | PayPal | Klarna

type Ticket = {
    ticketNr: TicketNumber
    ticketPrice: TicketPrice
    ticketType: TicketType
    departure: DepartureTrainStation
    arrival: ArrivalTrainStation
    departureTime: DepartureTime
    arrivalTime: ArrivalTime
}

type ShoppingCartEntry = {
    ticket: Ticket
    quantity: TicketQuantity
}

type PaidCartEntry = {
    ticket: Ticket
    quantity: TicketQuantity
    orderDate: DateTime
    paymentMethod: PaymentMethod
}

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

type GetTrainStations = list<TrainStation>
type PrintStations = list<TrainStation> -> unit

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

let init () : State =
    0

let update (msg : Message) (model : State) : State =
    match msg with
    | Increment -> model + 1
    | Decrement -> model - 1
    | IncrementBy x -> model + x
    | DecrementBy x -> model - x
