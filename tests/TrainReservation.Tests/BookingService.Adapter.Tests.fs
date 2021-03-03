module TrainReservation.Tests.BookingService.Adapter

open Xunit
open FsUnit.Xunit
open TrainReservation.BookingService.Adapter
open TrainReservation.Types

/// Fixtures
let unreserved_a1 =
    { SeatId = SeatId "1A"
      SeatDetail =
          { Coach = "A"
            SeatNumber = "1"
            BookingReference = "" } }

let unreserved_a2 =
    { SeatId = SeatId "2A"
      SeatDetail =
          { Coach = "A"
            SeatNumber = "2"
            BookingReference = "" } }

let seats_allocation_2 =
    { TrainId = TrainId "local_1000"
      Seats = [ unreserved_a1; unreserved_a2 ] }


[<Fact>]
let ``Request a booking reference for a seat allocation``() =
    let bookingReferenceService = bookingReferenceService "http://localhost:8082"

    let reference = bookingReferenceService seats_allocation_2

    let bookingId = (System.DateTime.Now.ToString "yyyy-MM-dd") + "-local_1000-1A-2A"

    let expected = (asConfirmedReservation (BookingId bookingId) seats_allocation_2)

    reference |> Result.map (should equal expected)
