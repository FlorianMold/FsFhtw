# Temporary title

## Commands

Known Commands:
- Help: Prints all known commands
- ShowTrainStations: Shows all available train-stations
- SearchTrips <from> <to>: Shows a list of scheduled train departures with a ticket-number and train-type.
- RequestTicket <ticketNr>: Show ticket information for ticket-nr. (from, to, price, departure, train-type, ticket-type)
- AddToCart <ticketNr> <amount> <ticketType>: Puts the ticket inside a cart.
- PrintCart: Shows all tickets, departures with prices.
- RemoveFromCart <ticketNr> <ticketType> <quantity>: Removes the ticket with the ticket-number and the ticket-type from the cart.
- ClearCart: Removes all tickets from the cart.
- PayCart <payment-method>: Pays the items in the cart (randomly fails on some payment-methods).
- ShowPaidOrders: Shows all paid orders of the past.

## Types
```f#
type Station = Vienna | Linz | Graz | ...
type TrainType = Railjet | NightJet | CityJet | ...
type TicketNr = int
type TicketType = SeniorTicket | JuniorTicket | AdultTicket | PetTicket
type TicketQuantity = int
type PaymentMethod = VISA | PayPal | Klarna | ...
```

## Example

### Displaying all train-stations
```shell
~ PrintTrainStations

vienna, linz, graz, salzburg, eisenstadt, bregenz, innsbruck, klagenfurt, St. poelten
```

### Searching for a ticket

```shell
# SearchTrips <from: Station> <to: Station>
~ SearchTrips vienna linz

--------------------------------------------------
| ticket-nr | from | to | departure | trainType |
--------------------------------------------------
| 1 | vienna | linz | 15.01.2022 11:00 | RailJet |
| 2 | vienna | linz | 15.01.2022 12:00 | RailJet |
| 3 | vienna | linz | 15.01.2022 13:00 | RailJet |
| 4 | vienna | linz | 15.01.2022 14:00 | RailJet |
--------------------------------------------------
```

### Request a ticket

```shell
# RequestTicket <ticketNr: TicketNr>
~ RequestTicket 1

------------------------------------------------------------------------
| ticketNr | from | to | price | departure | train-type | ticketType |
------------------------------------------------------------------------
| 1 | vienna | linz | '10$' | 15.01.2022 11:00 | Railjet | AdultTicket |
| 1 | vienna | linz | '5$' | 15.01.2022 11:00 | Railjet | JuniorTicket |
| 1 | vienna | linz | '7$' | 15.01.2022 11:00 | Railjet | SeniorTicket |
------------------------------------------------------------------------
```

### Adding a ticket

```shell
# AddToCart <ticketNr: TicketNr> <amount: TicketQuantity> <ticketType: TicketType>
~ AddToCart 1 2 SeniorTicket

Ticket-number '1' with price '7$' was added to cart.
```

### Showing the tickets
```shell
# PrintCart
~ PrintCart

---------------------------------------------------------------------------------
| ticketNr | from | to | price | departure | train-type | ticketType | quantity |
---------------------------------------------------------------------------------
| 1 | vienna | linz | '10$' | 15.01.2022 11:00 | Railjet | AdultTicket | 1 |
| 1 | vienna | linz | '5$' | 15.01.2022 11:00 | Railjet | JuniorTicket | 2 |
---------------------------------------------------------------------------------
```

### Remove a ticket from cart

```shell
# RemoveFromCart <ticketNr: TicketNr> <ticketType: TicketType> <quantity: TicketQuantity>
~ RemoveFromCart 1 JuniorTicket 1

 --------------------------------------------------------------------------------
| ticketNr | from | to | price | departure | train-type | ticketType | quantity |
---------------------------------------------------------------------------------
| 1 | vienna | linz | '10$' | 15.01.2022 11:00 | Railjet | AdultTicket | 1 |
| 1 | vienna | linz | '5$' | 15.01.2022 11:00 | Railjet | JuniorTicket | 1 |
---------------------------------------------------------------------------------

~ RemoveFromCart 1 JuniorTicket 1

---------------------------------------------------------------------------------
| ticketNr | from | to | price | departure | train-type | ticketType | quantity |
---------------------------------------------------------------------------------
| 1 | vienna | linz | '10$' | 15.01.2022 11:00 | Railjet | AdultTicket | 1 |
---------------------------------------------------------------------------------
```

### Clear the cart

```shell
# ClearCart
~ ClearCart

Following items were removed from the cart:
---------------------------------------------------------------------------------
| ticketNr | from | to | price | departure | train-type | ticketType | quantity |
---------------------------------------------------------------------------------
| 1 | vienna | linz | '10$' | 15.01.2022 11:00 | Railjet | AdultTicket | 1 |
---------------------------------------------------------------------------------
```

### Pay the cart

```shell
# PayCart <payment-method: PaymentMethod>
~ PayCart PayPal

Following tickets have been bought by PayPal:
---------------------------------------------------------------------------------
| ticketNr | from | to | price | departure | train-type | ticketType | quantity |
---------------------------------------------------------------------------------
| 1 | vienna | linz | '10$' | 15.01.2022 11:00 | Railjet | AdultTicket | 1 |
---------------------------------------------------------------------------------
Total: '10$'

# Fail Case
~ PayCart VISA

Your credit-card has not been accepted. Please use another payment-method.
```

### Show paid orders

```shell
# ShowPaidOrders
~ ShowPaidOrders

---------------------------------------------------------------------------------------------------------
| ticketNr | from | to | price | departure | train-type | ticketType | quantity | date | payment-method |
---------------------------------------------------------------------------------------------------------
| 1 | vienna | linz | '10$' | 15.01.2022 11:00 | Railjet | AdultTicket | 1 | '15.01.2022' | PayPal |
| 1 | vienna | linz | '7$' | 15.01.2022 11:00 | Railjet | AdultTicket | 1 | '14.01.2022' | Klarna |
---------------------------------------------------------------------------------------------------------
```
