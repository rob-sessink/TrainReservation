namespace TrainReservation.Tests

module AvailabilityFixtures =

    open TrainReservation.Types

    /// Fixtures and Helpers
    let standard_allocation_settings =
        { AllowedCapacity = Percentage 70m
          AllowedCoachCapacity = Percentage 100m
          Allotment = GroupedPerCoach }

    let to_train trainId seats: TrainInformation =
        { TrainId = TrainId trainId
          Seats = seats }

    let seats_allocated_66 =
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

        let unreserved_a3 =
            { SeatId = SeatId "3A"
              SeatDetail =
                  { Coach = "A"
                    SeatNumber = "3"
                    BookingReference = "" } }

        [ reserved_a1
          reserved_a2
          unreserved_a3 ]

    let seats_allocated_100 =
        let b1 =
            { SeatId = SeatId "1B"
              SeatDetail =
                  { Coach = "B"
                    SeatNumber = "1"
                    BookingReference = "210201-1B-2B-3B" } }

        let b2 =
            { SeatId = SeatId "2B"
              SeatDetail =
                  { Coach = "B"
                    SeatNumber = "2"
                    BookingReference = "210201-1B-2B-3B" } }

        let b3 =
            { SeatId = SeatId "3B"
              SeatDetail =
                  { Coach = "B"
                    SeatNumber = "3"
                    BookingReference = "210201-1B-2B-3B" } }

        [ b1; b2; b3 ]

    let train_coaches_A66_B100 = seats_allocated_100 @ seats_allocated_66
