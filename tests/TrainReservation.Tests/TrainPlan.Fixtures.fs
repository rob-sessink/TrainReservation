namespace TrainReservation.Tests

open System
open TrainReservation.Types
open TrainReservation.Types.Allocation

module TrainPlanFixtures =

    let reservationId1 = ReservationId.With(Guid("11111111-1111-1111-1111-111111111111"))

    let default_allocation_settings =
        { AllowedCapacity = Percentage 70m
          AllowedCoachCapacity = Percentage 100m
          Allotment = GroupedPerCoach }

    // 3 seats - 66% allocated
    let seats3_66Pct =
        let reserved_a1 =
            { SeatId = SeatId "1A"
              SeatDetail =
                  { Coach = CoachId "A"
                    SeatNumber = "1"
                    BookingReference = BookingReference.Create "210201-1A-2A"
                    ReservationId = ReservationId.With(Guid("11111111-1111-1111-1111-111111111111")) } }

        let reserved_a2 =
            { SeatId = SeatId "2A"
              SeatDetail =
                  { Coach = CoachId "A"
                    SeatNumber = "2"
                    BookingReference = BookingReference.Create "210201-1A-2A"
                    ReservationId = ReservationId.With(Guid("11111111-1111-1111-1111-111111111112")) } }

        let unreserved_a3 =
            { SeatId = SeatId "3A"
              SeatDetail =
                  { Coach = CoachId "A"
                    SeatNumber = "3"
                    BookingReference = BookingReference.Empty
                    ReservationId = ReservationId.Empty } }

        [ reserved_a1
          reserved_a2
          unreserved_a3 ]

    // 3 seats - 100% allocated
    let seats3_100Pct =
        let b1 =
            { SeatId = SeatId "1B"
              SeatDetail =
                  { Coach = CoachId "B"
                    SeatNumber = "1"
                    BookingReference = BookingReference.Create "210201-1B-2B-3B"
                    ReservationId = ReservationId.With(Guid("11111111-1111-1111-1111-111111111111")) } }

        let b2 =
            { SeatId = SeatId "2B"
              SeatDetail =
                  { Coach = CoachId "B"
                    SeatNumber = "2"
                    BookingReference = BookingReference.Create "210201-1B-2B-3B"
                    ReservationId = ReservationId.With(Guid("11111111-1111-1111-1111-111111111112")) } }

        let b3 =
            { SeatId = SeatId "3B"
              SeatDetail =
                  { Coach = CoachId "B"
                    SeatNumber = "3"
                    BookingReference = BookingReference.Create "210201-1B-2B-3B"
                    ReservationId = ReservationId.With(Guid("11111111-1111-1111-1111-111111111113")) } }

        [ b1; b2; b3 ]

    // 2 seats - 0% allocated
    let seats2_0Pct =
        let unreserved_c1 =
            { SeatId = SeatId "1C"
              SeatDetail =
                  { Coach = CoachId "C"
                    SeatNumber = "1"
                    BookingReference = BookingReference.Empty
                    ReservationId = ReservationId.Empty } }

        let unreserved_c2 =
            { SeatId = SeatId "2C"
              SeatDetail =
                  { Coach = CoachId "C"
                    SeatNumber = "2"
                    BookingReference = BookingReference.Empty
                    ReservationId = ReservationId.Empty } }

        let unreserved_c3 =
            { SeatId = SeatId "3C"
              SeatDetail =
                  { Coach = CoachId "C"
                    SeatNumber = "3"
                    BookingReference = BookingReference.Empty
                    ReservationId = ReservationId.Empty } }

        let unreserved_c4 =
            { SeatId = SeatId "4C"
              SeatDetail =
                  { Coach = CoachId "C"
                    SeatNumber = "4"
                    BookingReference = BookingReference.Empty
                    ReservationId = ReservationId.Empty } }

        [ unreserved_c1
          unreserved_c2
          unreserved_c3
          unreserved_c4 ]

    //  coaches A66% and B100%
    let coaches2_A66Pct_B100Pct = seats3_100Pct @ seats3_66Pct
