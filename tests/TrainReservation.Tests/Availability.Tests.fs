namespace TrainReservation.Tests

open Xunit
open FsUnit.Xunit

module Capacity =

    open TrainReservation.Types.Allocation
    open TrainReservation.Capacity

    type ``Capacity Tests``() =

        [<Fact>]
        let ``Calculate capacity when 67% allocated and 70% capacity`` () =

            let capacity = calculateCapacity (Percentage 70m) 30 20

            let expected =
                { Current = Percentage 67m
                  MaximumAllowed = Percentage 70m
                  Allocatable = Percentage 3m
                  UnitAllocatable = 1
                  UnitTotal = 30 }

            capacity |> should equal expected


        [<Fact>]
        let ``Calculate capacity when 0% allocated and 100% capacity`` () =

            let capacity = calculateCapacity (Percentage 100m) 150 0

            let expected =
                { Current = Percentage 0m
                  MaximumAllowed = Percentage 100m
                  Allocatable = Percentage 100m
                  UnitAllocatable = 150
                  UnitTotal = 150 }

            capacity |> should equal expected


        [<Fact>]
        let ``Calculate capacity when 100% allocated and 100% capacity`` () =

            let capacity = calculateCapacity (Percentage 100m) 150 150

            let expected =
                { Current = Percentage 100m
                  MaximumAllowed = Percentage 100m
                  Allocatable = Percentage 0m
                  UnitAllocatable = 0
                  UnitTotal = 150 }

            capacity |> should equal expected


        [<Fact>]
        let ``Calculate capacity when 49% allocated and 50% capacity`` () =

            let capacity = calculateCapacity (Percentage 50m) 100 49

            let expected =
                { Current = Percentage 49m
                  MaximumAllowed = Percentage 50m
                  Allocatable = Percentage 1m
                  UnitAllocatable = 1
                  UnitTotal = 100 }

            capacity |> should equal expected


        [<Fact>]
        let ``Calculate capacity when 60% allocated and 50% capacity`` () =

            let capacity = calculateCapacity (Percentage 50m) 100 60

            let expected =
                { Current = Percentage 60m
                  MaximumAllowed = Percentage 50m
                  Allocatable = Percentage -10M
                  UnitAllocatable = -10
                  UnitTotal = 100 }

            capacity |> should equal expected


module Availability =

    open TrainReservation.Tests.TrainPlanFixtures
    open TrainReservation.Availability
    open TrainReservation.Types
    open TrainReservation.Types.Allocation

    type ``Train Availability Tests``() =

        let plan3Seats_66Pct_Default = TrainPlan.Create "local_1000" seats3_66Pct default_allocation_settings

        let plan2Coaches_6Seats_A66Pct_B100Pct_Default =
            TrainPlan.Create "local_2000" coaches2_A66Pct_B100Pct default_allocation_settings

        [<Fact>]
        let ``Calculate seating capacity for the overall train`` () =

            let capacity = calculateTrainCapacity plan3Seats_66Pct_Default

            let expected =
                { Current = Percentage 67m
                  MaximumAllowed = Percentage 70m
                  Allocatable = Percentage 3m
                  UnitAllocatable = 0
                  UnitTotal = 3 }

            capacity |> should equal expected

        [<Fact>]
        let ``Calculate seating capacity per coach for a train`` () =

            let capacities = calculateCoachesCapacity plan2Coaches_6Seats_A66Pct_B100Pct_Default

            let expected =
                [ { Coach = CoachId "B"
                    Capacity =
                        { Current = Percentage 100M
                          MaximumAllowed = Percentage 100M
                          Allocatable = Percentage 0M
                          UnitAllocatable = 0
                          UnitTotal = 3 } }
                  { Coach = CoachId "A"
                    Capacity =
                        { Current = Percentage 67M
                          MaximumAllowed = Percentage 100M
                          Allocatable = Percentage 33M
                          UnitAllocatable = 1
                          UnitTotal = 3 } } ]

            capacities |> should equal expected


        [<Fact>]
        let ``When allocatable capacity is insufficient return availability: Unavailable`` () =
            let capacity =
                { Current = Percentage 67m
                  MaximumAllowed = Percentage 70m
                  Allocatable = Percentage 3m
                  UnitAllocatable = 0
                  UnitTotal = 3 }

            let availability = toAvailability (SeatCount 1) capacity

            let expected = Unavailable capacity

            availability |> should equal expected


        [<Fact>]
        let ``When allocatable capacity is available return availability: Available`` () =
            let capacity =
                { Current = Percentage 0m
                  MaximumAllowed = Percentage 70m
                  Allocatable = Percentage 70m
                  UnitAllocatable = 2
                  UnitTotal = 3 }

            let availability = toAvailability (SeatCount 2) capacity

            let expected = Available capacity

            availability |> should equal expected


        [<Fact>]
        let ``When maximum capacity is reached return availability: MaximumReached`` () =
            let capacity =
                { Current = Percentage 70m
                  MaximumAllowed = Percentage 70m
                  Allocatable = Percentage 0m
                  UnitAllocatable = 0
                  UnitTotal = 3 }

            let availability = toAvailability (SeatCount 2) capacity

            let expected = MaximumReached capacity

            availability |> should equal expected


        [<Fact>]
        let ``When maximum capacity is exceeded return availability: MaximumReached`` () =
            let capacity =
                { Current = Percentage 80m
                  MaximumAllowed = Percentage 70m
                  Allocatable = Percentage -10M
                  UnitAllocatable = 0
                  UnitTotal = 3 }

            let availability = toAvailability (SeatCount 2) capacity

            let expected = MaximumReached capacity

            availability |> should equal expected


        [<Fact>]
        let ``Single available seat for coach A`` () =
            let available = availableSeatsForCoach (CoachId "A") coaches2_A66Pct_B100Pct

            let expected =
                [ { SeatId = SeatId "3A"
                    SeatDetail =
                        { Coach = CoachId "A"
                          SeatNumber = "3"
                          ReservationId = ReservationId.Empty
                          BookingReference = BookingReference.Empty } } ]

            available |> should equal expected


        [<Fact>]
        let ``No available seats for coach B`` () =
            let available = availableSeatsForCoach (CoachId "B") coaches2_A66Pct_B100Pct

            let expected : Seat list = []
            available |> should equal expected
