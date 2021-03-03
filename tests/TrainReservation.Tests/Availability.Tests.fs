module TrainReservation.Tests.Availability

open TrainReservation.Tests.AvailabilityFixtures
open TrainReservation.Availability
open TrainReservation.Types
open Xunit
open FsUnit.Xunit

type ``Capacity Tests``() =

    [<Fact>]
    let ``Calculate capacity when 67% allocated and 70% capacity``() =

        let capacity = calculateCapacity (Percentage 70m) 30 20

        let expected =
            { Current = Percentage 67m
              MaximumAllowed = Percentage 70m
              Allocatable = Percentage 3m
              UnitAllocatable = 1
              UnitTotal = 30 }

        capacity |> should equal expected


    [<Fact>]
    let ``Calculate capacity when 0% allocated and 100% capacity``() =

        let capacity = calculateCapacity (Percentage 100m) 150 0

        let expected =
            { Current = Percentage 0m
              MaximumAllowed = Percentage 100m
              Allocatable = Percentage 100m
              UnitAllocatable = 150
              UnitTotal = 150 }

        capacity |> should equal expected


    [<Fact>]
    let ``Calculate capacity when 100% allocated and 100% capacity``() =

        let capacity = calculateCapacity (Percentage 100m) 150 150

        let expected =
            { Current = Percentage 100m
              MaximumAllowed = Percentage 100m
              Allocatable = Percentage 0m
              UnitAllocatable = 0
              UnitTotal = 150 }

        capacity |> should equal expected


    [<Fact>]
    let ``Calculate capacity when 49% allocated and 50% capacity``() =

        let capacity = calculateCapacity (Percentage 50m) 100 49

        let expected =
            { Current = Percentage 49m
              MaximumAllowed = Percentage 50m
              Allocatable = Percentage 1m
              UnitAllocatable = 1
              UnitTotal = 100 }

        capacity |> should equal expected


    [<Fact>]
    let ``Calculate capacity when 60% allocated and 50% capacity``() =

        let capacity = calculateCapacity (Percentage 50m) 100 60

        let expected =
            { Current = Percentage 60m
              MaximumAllowed = Percentage 50m
              Allocatable = Percentage -10M
              UnitAllocatable = -10
              UnitTotal = 100 }

        capacity |> should equal expected


    [<Fact>]
    let ``Calculate seating capacity for the overall train``() =

        let trainInformation = to_train "local_1000" seats_allocated_66

        let capacity = calculateTrainCapacity standard_allocation_settings trainInformation

        let expected =
            { Current = Percentage 67m
              MaximumAllowed = Percentage 70m
              Allocatable = Percentage 3m
              UnitAllocatable = 0
              UnitTotal = 3 }

        capacity |> should equal expected


type ``Train Availability Tests``() =

    [<Fact>]
    let ``Calculate seating capacity per coach for a train``() =

        let trainInformation = to_train "local_2000" train_coaches_A66_B100

        let capacities = calculateCoachesCapacity standard_allocation_settings trainInformation

        let expected =
            [ { Coach = "B"
                Capacity =
                    { Current = Percentage 100M
                      MaximumAllowed = Percentage 100M
                      Allocatable = Percentage 0M
                      UnitAllocatable = 0
                      UnitTotal = 3 } }
              { Coach = "A"
                Capacity =
                    { Current = Percentage 67M
                      MaximumAllowed = Percentage 100M
                      Allocatable = Percentage 33M
                      UnitAllocatable = 1
                      UnitTotal = 3 } } ]


        capacities |> should equal expected


    [<Fact>]
    let ``When allocatable capacity is insufficient return availability: Unavailable``() =
        let capacity =
            { Current = Percentage 67m
              MaximumAllowed = Percentage 70m
              Allocatable = Percentage 3m
              UnitAllocatable = 0
              UnitTotal = 3 }

        let availability = toAvailability 1 capacity

        let expected = Unavailable capacity

        availability |> should equal expected


    [<Fact>]
    let ``When allocatable capacity is available return availability: Available``() =
        let capacity =
            { Current = Percentage 0m
              MaximumAllowed = Percentage 70m
              Allocatable = Percentage 70m
              UnitAllocatable = 2
              UnitTotal = 3 }

        let availability = toAvailability 2 capacity

        let expected = Available capacity

        availability |> should equal expected


    [<Fact>]
    let ``When maximum capacity is reached return availability: MaximumReached``() =
        let capacity =
            { Current = Percentage 70m
              MaximumAllowed = Percentage 70m
              Allocatable = Percentage 0m
              UnitAllocatable = 0
              UnitTotal = 3 }

        let availability = toAvailability 2 capacity

        let expected = MaximumReached capacity

        availability |> should equal expected


    [<Fact>]
    let ``When maximum capacity is exceeded return availability: MaximumReached``() =
        let capacity =
            { Current = Percentage 80m
              MaximumAllowed = Percentage 70m
              Allocatable = Percentage -10M
              UnitAllocatable = 0
              UnitTotal = 3 }

        let availability = toAvailability 2 capacity

        let expected = MaximumReached capacity

        availability |> should equal expected


    [<Fact>]
    let ``Single available seat for coach A``() =

        let available = availableSeatsForCoach "A" train_coaches_A66_B100

        let expected =
            [ { SeatId = SeatId "3A"
                SeatDetail =
                    { Coach = "A"
                      SeatNumber = "3"
                      BookingReference = "" } } ]

        available |> should equal expected


    [<Fact>]
    let ``No available seats for coach B``() =

        let available = availableSeatsForCoach "B" train_coaches_A66_B100

        let expected: Seat list = []
        available |> should equal expected
