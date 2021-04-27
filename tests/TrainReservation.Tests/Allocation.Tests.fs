namespace TrainReservation.Tests

open System
open TrainReservation.Types.Allocation

module Allocation =

    open TrainReservation.Tests.TrainPlanFixtures
    open TrainReservation.Allocation
    open TrainReservation.Types
    open Xunit
    open FsUnit.Xunit

    let reservationId1 = ReservationId.With(Guid("11111111-1111-1111-1111-111111111111"))
    let reservationId4 = ReservationId.With(Guid("11111111-1111-1111-1111-111111111114"))

    let seatsUnreserved_A1 =
        { SeatId = SeatId "1A"
          SeatDetail =
              { Coach = CoachId "A"
                SeatNumber = "1"
                ReservationId = ReservationId.Empty
                BookingReference = BookingReference.Empty } }

    let seatsUnreserved_A2 =
        { SeatId = SeatId "2A"
          SeatDetail =
              { Coach = CoachId "A"
                SeatNumber = "2"
                ReservationId = ReservationId.Empty
                BookingReference = BookingReference.Empty } }

    let seatsUnreserved_A3 =
        { SeatId = SeatId "3A"
          SeatDetail =
              { Coach = CoachId "A"
                SeatNumber = "3"
                ReservationId = ReservationId.Empty
                BookingReference = BookingReference.Empty } }

    let seats_0Pct =
        [ seatsUnreserved_A1
          seatsUnreserved_A2
          seatsUnreserved_A3 ]

    let reserved_a1 =
        { SeatId = SeatId "1A"
          SeatDetail =
              { Coach = CoachId "A"
                SeatNumber = "1"
                ReservationId = reservationId1
                BookingReference = BookingReference.Empty } }

    let plan2Seats_0Pct_Default = TrainPlan.Create "local_1000" seats_0Pct default_allocation_settings
    let plan3Seats_100Pct_Default = TrainPlan.Create "local_1000" seats3_100Pct default_allocation_settings

    [<Fact>]
    let ``Try to allocate seats when capacity is available`` () =
        let requestFor1Seat = AllocationRequest.Create "local_1000" 1 reservationId1
        let expected = Allocation.Create "local_1000" [ reserved_a1 ] reservationId1

        let allocation = allocateSeats requestFor1Seat plan2Seats_0Pct_Default

        allocation |> Result.map (should equal expected)

    [<Fact>]
    let ``Try to allocate seats when capacity is unavailable`` () =
        let requestFor3Seats = AllocationRequest.Create "local_1000" 3 reservationId1

        let allocation = allocateSeats requestFor3Seats plan2Seats_0Pct_Default

        match allocation with
        | Error (NoSeatsAvailable _) -> ()
        | _ -> failwith "Expected ReservationError.NoSeatsAvailable"

    [<Fact>]
    let ``Try to allocate seats when maximum capacity was reached`` () =
        let requestFor1Seat = AllocationRequest.Create "local_1000" 1 reservationId4

        let allocation = allocateSeats requestFor1Seat plan3Seats_100Pct_Default

        match allocation with
        | Error (MaximumCapacityReached _) -> ()
        | _ -> failwith "Expected ReservationError.MaximumCapacityReached"
