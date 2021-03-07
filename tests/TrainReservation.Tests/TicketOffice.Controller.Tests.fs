module TrainReservation.Tests.TicketOffice.Controller

open TrainReservation.Reservation.Api.Types
open TrainReservation.Types
open TrainReservation.TicketOffice
open TrainReservation.TicketOffice.Controller
open TrainReservation.Tests.Fixtures
open TrainReservation.Tests.HttpContextUtil

open FSharp.Control.Tasks.V2
open Microsoft.AspNetCore.Http
open Newtonsoft.Json
open NSubstitute
open System.IO
open System.Text

open FsUnit.Xunit
open Xunit


/// ---------------------------------------------------------------------------
/// Thoth Decoder/Encoder Tests

[<Fact>]
let ``Decode reservation request from json`` () =
    let json =
        readFixture "fixtures/reservationRequest.json"

    let unvalidatedReservationRequest = decodeRequest json

    let expected: Result<UnvalidatedReservationRequest, ReservationError> =
        Ok
            { TrainId = "local_1000"
              SeatCount = 2 }

    unvalidatedReservationRequest
    |> should equal expected


[<Fact>]
let ``Encode reservation to json`` () =

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


/// ---------------------------------------------------------------------------
/// HttpHandler Tests
///
[<Fact>]
let ``POST request /reserve process the reservation request`` () =

    let request: ClientReservationRequest = { trainId = "local_1000"; seats = 1 }

    let postData =
        Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request))

    let context = buildMockContext ()
    context.Request.Body <- new MemoryStream(postData)

    context.Request.Method.ReturnsForAnyArgs "POST"
    |> ignore

    context.Request.Path.ReturnsForAnyArgs(PathString("/reserve"))
    |> ignore

    context.Request.Body <- new MemoryStream(postData)

    task {
        let! result = WebApp.webApp next context

        match result with
        | None -> failwith "Result was expected to be %s"
        | Some ctx ->
            getBody ctx
            |> should haveSubstring "\\\"seats\\\":[\\\"1A\\\"]"
    }
