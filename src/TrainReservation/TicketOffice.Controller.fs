namespace TrainReservation.TicketOffice

open Microsoft.AspNetCore.Http
open TrainReservation.Types
open TrainReservation.Types.Allocation

module Decoder =

    open Thoth.Json.Net

    /// <summary>Decoder of a reservation request</summary>
    /// <returns>UnvalidatedReservationRequest</returns>
    /// {
    ///   "trainId": "local_1000",
    ///   "seats": 2
    /// }
    let reservationRequestDecoder : Decoder<UnvalidatedReservationRequest> =
        Decode.object
            (fun get ->
                { TrainId = get.Required.Field "trainId" Decode.string
                  SeatCount = get.Required.Field "seats" Decode.int })

    let decodeRequest json =
        Decode.fromString reservationRequestDecoder json
        |> Result.mapError (fun e -> InvalidRequest $"Invalid reservation request: {e}")


module Encoder =

    open TrainReservation.ApplicationTime
    open TrainReservation.ApiTypes
    open Thoth.Json.Net

    /// <summary>Try encoding a reservation into a json string</summary>
    /// <param name="reservation">to encode</param>
    /// <returns>json</returns>
    let encodeReservation (reservation: Reservation) =
        Encode.object [ "train_id", Encode.string reservation.TrainId.Value
                        "booking_reference", Encode.string reservation.BookingId.Value
                        "seats",
                        reservation.Seats
                        |> List.map (fun s -> s.SeatId.Value |> Encode.string)
                        |> Encode.list ]
        |> Encode.toString 0

    /// <summary>Encode ReservationError to corresponding Http error response</summary>
    /// <param name="err">to map</param>
    /// <param name="ctx">of the request</param>
    /// <returns>Http error response</returns>
    let encodeResponseError err (ctx: HttpContext) =
        // partially applied first and piped-in last
        let error = error time.Now
        let path = ctx.Request.Path

        match err with
        | InvalidRequest e -> error 400 e
        | InvalidTrainId (_, e) -> error 400 e
        | InvalidSeatCount (_, e) -> error 400 e
        | TrainIdNotFound (_, e) -> error 400 e
        | UnallocatedTrainPlan e -> error 400 e
        | MissingAllocation e -> error 400 e
        | InvalidTrainPlan e -> error 500 e
        | PendingAllocationExist e -> error 500 e
        | NoSeatsAvailable _ -> error 500 "Not enough seats available."
        | NoCoachAvailable _ -> error 500 "No coach available to accomodate all seats."
        | MaximumCapacityReached _ -> error 500 "Maximum train capacity reached, no more seats available."
        <| path.Value


module Controller =

    open Decoder
    open Encoder
    open Giraffe
    open FSharp.Control.Tasks.V2.ContextInsensitive

    /// <summary>Handler for the reservation request</summary>
    /// <param name="reserveSeats">workflow handling the request</param>
    /// <param name="next">HTTP middleware function</param>
    /// <param name="ctx">of the HTTP request</param>
    /// <returns>ConfirmedReservation or an error Json representation</returns>
    let reservationHandler (reserveSeats: ReserveSeatsFlow) : HttpHandler =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                let! json = ctx.ReadBodyFromRequestAsync()
                let reservation = json |> decodeRequest >>= reserveSeats

                return!
                    match reservation with
                    | Error e -> (encodeResponseError e ctx) next ctx
                    | Ok r -> Successful.OK(encodeReservation r) next ctx
            }

    /// <summary>Handler to reset all reservations for a train</summary>
    /// <param name="resetReservations">workflow handling the request</param>
    /// <param name="trainId">to reset reservations for</param>
    /// <param name="next">HTTP middleware function</param>
    /// <param name="ctx">of the HTTP request</param>
    /// <returns>Ok</returns>
    let resetHandler (resetReservations: ResetReservationsFlow) (trainId: string) : HttpHandler =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            task {
                resetReservations { TrainId = trainId }

                return! Successful.OK $"Reservations for train: {trainId} were successfully reset." next ctx
            }
