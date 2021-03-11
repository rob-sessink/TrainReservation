namespace TrainReservation

module ReserveSeatsFlow =

    open TrainReservation.Types
    open TrainReservation.Allocation

    type ValidateReservationRequest = UnvalidatedReservationRequest -> Result<ValidReservationRequest, ReservationError>

    /// <summary>Validate a reservation request</summary>
    /// <param name="request">UnvalidatedReservationRequest</param>
    /// <returns>ValidatedReservationRequest</returns>
    let validateReservationRequest: ValidateReservationRequest =
        fun request ->

            // Push as into constrained type with applicative error handling?
            let validateTrainId (req: UnvalidatedReservationRequest) =
                match req.TrainId with
                | t when t.Length < 5 -> Error(InvalidTrainId(req, "Train identifier is invalid"))
                | _ -> Ok req

            let validateSeatCount (req: UnvalidatedReservationRequest) =
                match req.SeatCount with
                | c when c < 0 -> Error(InvalidSeatCount(req, "Seat count cannot be negative"))
                | c when c = 0 -> Error(InvalidSeatCount(req, "Seat count cannot be zero"))
                | _ -> Ok(req)

            let asValidReservationRequest (req: UnvalidatedReservationRequest) =
                Ok
                    { TrainId = TrainId req.TrainId
                      SeatCount = req.SeatCount }

            request
            |> validateTrainId
            >>= validateSeatCount
            >>= asValidReservationRequest


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
