module TrainReservation.ReserveSeatsFlow

open TrainReservation.Types
open TrainReservation.Allocation

type ValidateReservationRequest = UnvalidatedReservationRequest -> Result<ValidReservationRequest, ReservationError>

/// <summary>Validate a reservation request</summary>
/// <param name="request">UnvalidatedReservationRequest</param>
/// <returns>ValidatedReservationRequest</returns>
let validateReservationRequest: ValidateReservationRequest =
    fun request ->

        // Push as into constrained type with applicative error handling?
        let validateTrainId (request: UnvalidatedReservationRequest) =
            match request.TrainId with
            | t when t.Length < 5 -> Error(InvalidTrainId(request, "Train identifier is invalid"))
            | _ -> Ok request

        let validateSeatCount (request: UnvalidatedReservationRequest) =
            match request.SeatCount with
            | c when c < 0 -> Error(InvalidSeatCount(request, "Seat count cannot be negative"))
            | c when c = 0 -> Error(InvalidSeatCount(request, "Seat count cannot be zero"))
            | _ -> Ok(request)

        let validate request =
            request
            |> validateTrainId
            >>= validateSeatCount

        match validate request with
        | Error e -> Error e
        | Ok v ->
            Result.Ok
                { TrainId = TrainId v.TrainId
                  SeatCount = v.SeatCount }


/// Type containing all service dependencies
type IO =
    { ProvideTrainSeatingInformation: ProvideTrainSeatingInformation
      ProvideBookingReference: ProvideBookingReference
      UpdateTrainSeatingInformation: UpdateTrainSeatingInformation }


/// <summary>Reserve seats on a train</summary>
/// <param name="io">service dependencies</param>
/// <param name="unvalidatedRequest">for seat reservation to-be </param>
/// <returns>Confirmed Reservation</returns>
let reserveSeats io: ReserveSeatsFlow =
    fun unvalidatedRequest ->

        // inline function as request is needed as parameter in allocateSeats
        // refactor to use FsToolkit.ErrorHandling
        let reserve request =
            request
            |> io.ProvideTrainSeatingInformation
            >>= allocateSeats request
            >>= io.ProvideBookingReference
            >>= io.UpdateTrainSeatingInformation

        // publish domain event for the reservation to TrainDataService
        let reservation =
            unvalidatedRequest
            |> validateReservationRequest
            >>= reserve

        reservation
