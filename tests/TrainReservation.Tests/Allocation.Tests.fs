namespace TrainReservation.Tests

module Allocation =

    open TrainReservation.Tests.AvailabilityFixtures
    open TrainReservation.Allocation
    open TrainReservation.Types
    open Xunit
    open FsUnit.Xunit

    // Fixtures
    let unreserved_a1 =
        { SeatId = SeatId "1A"
          SeatDetail =
              { Coach = "A"
                SeatNumber = "1"
                BookingReference = BookingReference.Empty } }

    let unreserved_a2 =
        { SeatId = SeatId "2A"
          SeatDetail =
              { Coach = "A"
                SeatNumber = "2"
                BookingReference = BookingReference.Empty } }

    let unreserved_a3 =
        { SeatId = SeatId "3A"
          SeatDetail =
              { Coach = "A"
                SeatNumber = "3"
                BookingReference = BookingReference.Empty } }

    let seats_allocated_0 =
        [ unreserved_a1
          unreserved_a2
          unreserved_a3 ]

    [<Fact>]
    let ``Try to allocate seats when capacity is available`` () =

        let request =
            { TrainId = TrainId "local_1000"
              SeatCount = 1 }

        let trainInformation = to_train "local_1000" seats_allocated_0
        let allocation = allocateSeats request trainInformation

        let expected =
            { TrainId = TrainId "local_1000"
              Seats = [ unreserved_a1 ] }

        allocation |> Result.map (should equal expected)

    [<Fact>]
    let ``Try to allocate seats when capacity is unavailable`` () =

        let request =
            { TrainId = TrainId "local_1000"
              SeatCount = 3 }

        let trainInformation = to_train "local_1000" seats_allocated_0
        let allocation = allocateSeats request trainInformation

        match allocation with
        | Error (NoSeatsAvailable _) -> ()
        | _ -> failwith "Expected ReservationError.NoSeatsAvailable"

    [<Fact>]
    let ``Try to allocate seats when maximum capacity was reached`` () =

        let request =
            { TrainId = TrainId "local_1000"
              SeatCount = 1 }

        let trainInformation =
            to_train "local_1000" seats_allocated_100

        let allocation = allocateSeats request trainInformation

        match allocation with
        | Error (MaximumCapacityReached _) -> ()
        | _ -> failwith "Expected ReservationError.MaximumCapacityReached"
