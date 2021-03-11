namespace TrainReservation.Tests

module ReserveSeatsFlow =

    open TrainReservation.Types
    open TrainReservation.Root
    open TrainReservation.ReserveSeatsFlow
    open Xunit
    open FsUnit.Xunit

    type ``ReserveSeats Flow Scenario Tests against mocked data services``() =

        /// Fixtures
        let request: UnvalidatedReservationRequest =
            { TrainId = "local_1000"
              SeatCount = 1 }

        let confirmed_reservation =
            let bookingId =
                (System.DateTime.Now.ToString "yyyy-MM-dd")
                + "-local_1000-1A"

            let reserved_a1 =
                { SeatId = SeatId "1A"
                  SeatDetail =
                      { Coach = "A"
                        SeatNumber = "1"
                        BookingReference = bookingId } }

            { TrainId = TrainId "local_1000"
              BookingId = BookingId bookingId
              Seats = [ reserved_a1 ] }

        let reserveSeat = ComposeReserveSeatsFlow


        [<Fact>]
        let ``Confirmed: try reserving seats for an allocatable request and receive confirmation`` () =
            let reservation = reserveSeat request

            reservation
            <!> (should equal confirmed_reservation)


        [<Fact>]
        let ``NoSeatsAvailable: try reserving seats for an un-allocatable request and receive back an error`` () =
            match reserveSeat { request with SeatCount = 100 } with
            | Error (NoSeatsAvailable _) -> ()
            | _ -> failwith "Expected ReservationError.NoSeatsAvailable"


        //     need to adjust maximum capacity as now 70M results in Availability.Unavailable
        //    [<Fact>]
        //    let ``NoCoachAvailable: try reserving seats for an un-allocatable request because no coach is available and receive an error`` () =
        //        let request: UnvalidatedReservationRequest =
        //            { TrainId = "inter_4000"
        //              SeatCount = 2 }
        //
        //        match reserveSeat request with
        //        | Error (NoCoachAvailable _) -> ()
        //        | _ -> failwith "Expected ReservationError.NoCoachAvailable"


        [<Fact>]
        let ``MaximumCapacityReached: try reserving seats for an allocatable request and receive an error`` () =
            match reserveSeat { request with TrainId = "inter_3000" } with
            | Error (MaximumCapacityReached _) -> ()
            | _ -> failwith "Expected ReservationError.MaximumCapacityReached"


        [<Fact>]
        let ``TrainIdNotFound: try reserving seats for an unknown train`` () =
            match reserveSeat { request with TrainId = "local_99999" } with
            | Error (TrainIdNotFound _) -> ()
            | _ -> failwith "Expected ReservationError.TrainIdNotFound"


    type ``ReservationRequest Validation Tests``() =

        [<Fact>]
        let ``Validate a correct reservation request `` () =
            let request: UnvalidatedReservationRequest =
                { TrainId = "local_1000"
                  SeatCount = 2 }

            let result = validateReservationRequest request

            let expected: ValidReservationRequest =
                { TrainId = TrainId "local_1000"
                  SeatCount = 2 }

            result <!> (should equal expected)


        [<Fact>]
        let ``Validate an incorrect reservation request with invalid a trainId`` () =
            let request: UnvalidatedReservationRequest = { TrainId = ""; SeatCount = 1 }

            let result = validateReservationRequest request

            let expected =
                InvalidTrainId({ TrainId = ""; SeatCount = 1 }, "Train identifier is invalid")

            Result.mapError (should equal expected) result


        [<Fact>]
        let ``Validate an incorrect reservation request with zero seats`` () =
            let request: UnvalidatedReservationRequest =
                { TrainId = "local_1000"
                  SeatCount = 0 }

            let result = validateReservationRequest request

            let expected =
                (InvalidSeatCount
                    ({ TrainId = "local_1000"
                       SeatCount = 0 },
                     "Seat count cannot be zero"))

            Result.mapError (should equal expected) result


        [<Fact>]
        let ``Validate an incorrect reservation request with negative seats`` () =
            let request: UnvalidatedReservationRequest =
                { TrainId = "local_1000"
                  SeatCount = -1 }

            let result = validateReservationRequest request

            let expected =
                (InvalidSeatCount
                    ({ TrainId = "local_1000"
                       SeatCount = -1 },
                     "Seat count cannot be negative"))

            Result.mapError (should equal expected) result
