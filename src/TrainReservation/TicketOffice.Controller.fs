module TrainReservation.TicketOffice.Controller

open TrainReservation.Types
open Microsoft.AspNetCore.Http
open Thoth.Json.Net
open Giraffe
open FSharp.Control.Tasks.V2.ContextInsensitive

/// <summary>Decoder of a reservation request</summary>
/// <returns>UnvalidatedReservationRequest</returns>
/// {
///   "trainId": "local_1000",
///   "seats": 2
/// }
let reservationRequestDecoder: Decoder<UnvalidatedReservationRequest> =
    Decode.object (fun get ->
        { TrainId = get.Required.Field "trainId" Decode.string
          SeatCount = get.Required.Field "seats" Decode.int })

let decodeRequest json =
    match Decode.fromString reservationRequestDecoder json with
    | Ok r -> Ok r
    | Error e -> Error(InvalidRequest $"Invalid reservation request: {e}")

/// <summary>Try encoding a reservation into a json string</summary>
/// <param name="reservation">to encode</param>
/// <returns>json</returns>
let encodeReservation (reservation: ConfirmedReservation) =
    Encode.object [ "train_id", Encode.string (TrainId.value reservation.TrainId)
                    "booking_reference", Encode.string (BookingId.value reservation.BookingId)
                    "seats",
                    reservation.Seats
                    |> List.map (fun s -> SeatId.value s.SeatId |> Encode.string)
                    |> Encode.list ]
    |> Encode.toString 0


let errorMapper error =
    match error with
    | InvalidRequest e -> RequestErrors.BAD_REQUEST e
    | InvalidTrainId (_, e) -> RequestErrors.BAD_REQUEST e
    | InvalidSeatCount (_, e) -> RequestErrors.BAD_REQUEST e
    | TrainIdNotFound (_, e) -> RequestErrors.BAD_REQUEST e
    | InvalidTrainInformation e -> ServerErrors.INTERNAL_ERROR e
    | NoSeatsAvailable _ -> ServerErrors.INTERNAL_ERROR "Not enough seats available."
    | NoCoachAvailable _ -> ServerErrors.INTERNAL_ERROR "No coach available to accomodate all seats."
    | MaximumCapacityReached _ -> ServerErrors.INTERNAL_ERROR "Maximum train capacity reached, no more seats available."

/// <summary>Handler for the reservation request</summary>
let reservationHandler (reserveSeats: ReserveSeatsFlow): HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            let! json = ctx.ReadBodyFromRequestAsync()
            let reservation = json |> decodeRequest >>= reserveSeats

            return!
                match reservation with
                | Error e -> (errorMapper e) next ctx
                | Ok r -> Successful.OK (encodeReservation r) next ctx
        }

/// <summary>Handler to reset all reservations for a train</summary>
let resetHandler (resetReservations: ResetReservationsFlow) (trainId: string): HttpHandler =
    fun (next: HttpFunc) (ctx: HttpContext) ->
        task {
            resetReservations { TrainId = trainId }

            return! Successful.OK $"Reservations for train: {trainId} were successfully reset." next ctx
        }
