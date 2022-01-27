module FsFhtw.Tests

open Domain
open Xunit
open FsCheck

[<Fact>]
let ``That the laws of reality still apply`` () = Assert.True(1 = 1)

[<Fact>]
let ``That searching for a valid trip yields a result`` () =
    let initialState = Domain.init ()

    let actual =
        Domain.update
            (Domain.SearchTrips
                { departure = "Vienna"
                  arrival = "Linz" })
            initialState

    let (_, stations, _, _, _) = actual

    Assert.True(stations.Length > 0)

[<Fact>]
let ``That searching for an invalid trip yields no result`` () =
    let initialState = Domain.init ()

    let actual =
        Domain.update
            (Domain.SearchTrips
                { departure = "Bregenz"
                  arrival = "Vienna" })
            initialState

    let (_, stations, _, _, _) = actual

    Assert.True(stations.Length = 0)

[<Fact>]
let ``That requesting for a valid trip yields a result`` () =
    let initialState = Domain.init ()

    let actual =
        Domain.update
            (Domain.RequestTicket
                { departure = "Vienna"
                  arrival = "Linz" })
            initialState

    let (_, stations, _, _, _) = actual

    Assert.True(stations.Length > 0)

[<Fact>]
let ``That requesting for an invalid trip yields no result`` () =
    let initialState = Domain.init ()

    let actual =
        Domain.update
            (Domain.RequestTicket
                { departure = "Bregenz"
                  arrival = "Vienna" })
            initialState

    let (_, stations, _, _, _) = actual

    Assert.True(stations.Length = 0)

[<Fact>]
let ``Adding a ticket to the cart`` () =
    let initialState = Domain.init ()

    let actual =
        Domain.update
            (Domain.AddTicketToCart
                { ticketNr = 100
                  ticketType = TicketType.AdultTicket
                  ticketQuantity = 1
                  })
            initialState

    let (cart, _, _, _, _) = actual

    Assert.True(cart.tickets.Length > 0)

[<Fact>]
let ``Trying to add an invalid ticket to the cart`` () =
    let initialState = Domain.init ()

    let actual =
        Domain.update
            (Domain.AddTicketToCart
                { ticketNr = 1
                  ticketType = TicketType.AdultTicket
                  ticketQuantity = 1
                  })
            initialState

    let (cart, _, _, _, _) = actual

    Assert.True(cart.tickets.Length = 0)

[<Fact>]
let ``Removing a ticket from the cart`` () =
    let initialState = Domain.init ()

    let actual =
        Domain.update
            (Domain.AddTicketToCart
                { ticketNr = 100
                  ticketType = TicketType.AdultTicket
                  ticketQuantity = 1
                  })
            initialState

    let (cart, _, _, _, _) = actual

    Assert.True(cart.tickets.Length > 0)

    let actual =
        Domain.update
            (Domain.RemoveTicketFromCart
                { ticketNr = 100
                  ticketType = TicketType.AdultTicket
                  ticketQuantity = 1
                  })
            initialState

    let (cart, _, _, _, _) = actual

    Assert.True(cart.tickets.Length = 0)

