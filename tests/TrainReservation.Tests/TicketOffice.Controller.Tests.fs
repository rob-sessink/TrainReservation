module TrainReservation.Tests.TicketOffice.Controller

open TrainReservation.Types
open TrainReservation.TicketOffice.Controller
open TrainReservation.Tests.Fixtures
open Xunit
open FsUnit.Xunit

[<Fact>]
let ``Decode reservation request from json``() =
    let json = readFixture "fixtures/reservationRequest.json"

    let unvalidatedReservationRequest = decodeRequest json

    let expected: Result<UnvalidatedReservationRequest, ReservationError> =
        Ok
            { TrainId = "local_1000"
              SeatCount = 2 }

    unvalidatedReservationRequest |> should equal expected


[<Fact>]
let ``Encode reservation to json``() =

    let reserved_a1 =
        { SeatId = SeatId "1A"
          SeatDetail =
              { Coach = "A"
                SeatNumber = "1"
                BookingReference = "210201-1A-2A" } }

    let reserved_a2 =
        { SeatId = SeatId "2A"
          SeatDetail =
              { Coach = "A"
                SeatNumber = "2"
                BookingReference = "210201-1A-2A" } }

    let confirmed_reservation =
        { TrainId = TrainId "local_1000"
          BookingId = BookingId "2021-02-15-local_1000-1A-2A"
          Seats = [ reserved_a1; reserved_a2 ] }

    let json = encodeReservation confirmed_reservation

    let expected = readFixture "fixtures/reservation.json"

    json |> should equal expected


//[<Fact>]
//let ``Reservation Handlers `` () =
//    let fakeReserveSeats = Ok reservation
//
//    let reservationHandler =
//        TrainReservation.TicketOffice.Controller.reservationHandler fakeReserveSeats
