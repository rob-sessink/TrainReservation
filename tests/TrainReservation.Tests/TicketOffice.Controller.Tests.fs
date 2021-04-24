namespace TrainReservation.Tests.TicketOffice

open System
open TrainReservation.Tests.Fixtures
open FsUnit.Xunit
open Xunit

module Decoder =

    open TrainReservation.TicketOffice.Decoder
    open TrainReservation.Types.Allocation

    /// ---------------------------------------------------------------------------
    /// Thoth Decoder/Encoder Tests

    [<Fact>]
    let ``Decode reservation request from json`` () =
        let json = readFixture "fixtures/reservationRequest.json"

        let unvalidatedReservationRequest = decodeRequest json

        let expected : Result<UnvalidatedReservationRequest, AllocationError> =
            Ok
                { TrainId = "local_1000"
                  SeatCount = 2 }

        unvalidatedReservationRequest |> should equal expected

module Encoder =

    open TrainReservation.TicketOffice.Encoder
    open TrainReservation.Types
    open TrainReservation.Types.Allocation

    [<Fact>]
    let ``Encode reservation to json`` () =

        let reserved_a1 =
            { SeatId = SeatId "1A"
              SeatDetail =
                  { Coach = "A"
                    SeatNumber = "1"
                    ReservationId = ReservationId.With(Guid.Empty)
                    BookingReference = BookingReference.Create "210201-1A-2A" } }

        let reserved_a2 =
            { SeatId = SeatId "2A"
              SeatDetail =
                  { Coach = "A"
                    SeatNumber = "2"
                    ReservationId = ReservationId.With(Guid.Empty)
                    BookingReference = BookingReference.Create "210201-1A-2A" } }

        let confirmed_reservation =
            { TrainId = TrainId "local_1000"
              BookingId = BookingId "2021-02-15-local_1000-1A-2A"
              Seats = [ reserved_a1; reserved_a2 ] }

        let json = encodeReservation confirmed_reservation

        let expected = readFixture "fixtures/reservation.json"

        json |> should equal expected


module Controller =

    open TrainReservation.ApiTypes
    open TrainReservation.TicketOffice
    open TrainReservation.Tests.HttpContextUtil
    open FSharp.Control.Tasks.V2

    /// ---------------------------------------------------------------------------
    /// HttpHandler Tests

    [<Fact>]
    let ``POST a reservation request to '/reserve' receiving a confirmed reservation`` () =

        let request = Some { trainId = "local_1000"; seats = 1 }

        let ctx = buildHandlerContext "POST" "/reserve" request

        task {
            let! result = WebApp.webApp next ctx

            match result with
            | Some ctx -> ctx.Response.StatusCode |> should equal 200
            | None -> failwith "Expected a context"
        }

    [<Fact>]
    let ``POST a invalid reservation request to '/reserve' and receive a bad request error`` () =

        let request = Some { trainId = "local_1000"; seats = -1 }

        let ctx = buildHandlerContext "POST" "/reserve" request

        task {
            let! result = WebApp.webApp next ctx

            match result with
            | Some ctx -> ctx.Response.StatusCode |> should equal 400
            | None -> failwith "Expected a context"
        }

    [<Fact>]
    let ``POST a valid reset reservation request to '/reset' and receive an 200`` () =

        let ctx = buildHandlerContext "POST" "/reset/local_1000" None

        task {
            let! result = WebApp.webApp next ctx

            match result with
            | Some ctx -> ctx.Response.StatusCode |> should equal 200
            | None -> failwith "Expected a context"
        }

    [<Fact>]
    let ``POST a unknown request '/unknown' and receive back an 404`` () =

        let ctx = buildHandlerContext "POST" "/unknown" None

        task {
            let! result = WebApp.webApp next ctx

            match result with
            | Some ctx -> ctx.Response.StatusCode |> should equal 404
            | None -> failwith "Expected a context"
        }
