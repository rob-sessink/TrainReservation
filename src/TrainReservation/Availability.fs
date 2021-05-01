namespace TrainReservation

module Capacity =

    open System
    open TrainReservation.Types.Allocation

    type CalculateCapacity = Percentage -> int -> int -> Capacity

    ///<summary>Calculate capacity</summary>
    ///<param name="maximumAllowed">allowed capacity as percentage</param>
    ///<param name="unitTotal">total number of allocatable units</param>
    ///<param name="unitAllocated">number of units already allocated</param>
    ///<returns>calculated capacity</returns>
    let calculateCapacity : CalculateCapacity =
        fun maximumAllowed unitTotal unitAllocated ->
            let total = decimal unitTotal
            let allocated = decimal unitAllocated

            // (1.m - (30.m - 20.m) / 30.m) * 100.m = 67.666
            // 67.666 -> rounded down = 67
            let current = (1.m - ((total - allocated) / total)) * 100.m |> Math.Round

            // 70 - 67 = 3
            let allocatable = maximumAllowed.Value - current

            // 30.m * (3.m / 100.m) -> rounded down = 0
            let unitAllocatable = total * (allocatable / 100.m) |> Math.Round |> int

            { Current = current |> Percentage
              MaximumAllowed = maximumAllowed
              Allocatable = allocatable |> Percentage
              UnitAllocatable = unitAllocatable
              UnitTotal = unitTotal }

    /// Active Patterns used in determining availability based upon capacity and number of requested seats
    let (|UnavailableFor|_|) requested c =
        match c.Allocatable > Percentage 0m && c.UnitAllocatable < requested with
        | true -> Some c
        | false -> None

    let (|AvailableFor|_|) requested c =
        match c.Allocatable > Percentage 0m && c.UnitAllocatable >= requested with
        | true -> Some c
        | false -> None

    let (|MaximumReachedFor|_|) c =
        match c.Allocatable <= Percentage 0m with
        | true -> Some c
        | false -> None


module Availability =

    open Capacity
    open TrainReservation.Types
    open TrainReservation.Types.Allocation

    /// <summary>Get all available seats in coach</summary>
    let availableSeatsForCoach coach seats =
        seats
        |> List.filter (fun s -> s.SeatDetail.Coach = coach && not s.SeatDetail.ReservationId.Exists)

    /// <summary>Count seats allocated in a train</summary>
    let countAllocatedSeats seats =
        seats |> List.filter (fun s -> s.SeatDetail.ReservationId.Exists) |> List.length

    type CalculateCoachesCapacity = TrainPlan -> CoachCapacity list

    /// <summary>Calculate the capacity for every coach in a train</summary>
    /// <param name="plan">TrainInformation</param>
    /// <returns>list of tuples with the capacity per coach</returns>
    let calculateCoachesCapacity : CalculateCoachesCapacity =
        fun plan ->
            let seatCapacity settings seats =
                let total = seats |> List.length
                let allocated = countAllocatedSeats seats
                calculateCapacity settings.AllowedCoachCapacity total allocated

            let coachesCapacity =
                plan.Seats
                |> List.groupBy (fun seat -> seat.SeatDetail.Coach)
                |> List.map
                    (fun s ->
                        { Coach = fst s
                          Capacity = seatCapacity plan.AllocationSettings (snd s) })

            coachesCapacity

    type CalculateTrainCapacity = TrainPlan -> Capacity

    /// <summary>Calculate the capacity for a train plan</summary>
    /// <param name="plan">TrainInformation</param>
    /// <returns>Capacity</returns>
    let calculateTrainCapacity : CalculateTrainCapacity =
        fun plan ->
            let allowed = plan.AllocationSettings.AllowedCapacity
            let total = plan.Seats |> List.length
            let allocated = countAllocatedSeats plan.Seats
            calculateCapacity allowed total allocated

    /// <summary>Determine availability based on capacity and seats requested</summary>
    /// <param name="requested">number of seats</param>
    /// <param name="capacity">of train or coach</param>
    /// <returns>Availability</returns>
    let toAvailability (requested: SeatCount) capacity =
        match capacity with
        | UnavailableFor requested.Value c -> Unavailable c
        | AvailableFor requested.Value c -> Available c
        | MaximumReachedFor c -> MaximumReached c
        | _ -> Unavailable capacity

    type AllotmentStrategy = AllocationRequest -> TrainPlan -> Result<Seat list, AllocationError>

    let availableCoach coach =
        match coach.Availability with
        | Available _ -> Some coach
        | _ -> None

    let asCoachAvailability seatCount (coach: CoachCapacity) =
        { Coach = coach.Coach
          Availability = toAvailability seatCount coach.Capacity }

    /// <summary>Strategy to find a single coach that allows the grouped allotment of the requested seats</summary>
    /// <param name="request">for the reservation of seats</param>
    /// <param name="plan">train seating and occupation details</param>
    /// <returns>allocatable coach </returns>
    /// https://stackoverflow.com/questions/34120591/f-generic-function-filter-list-of-discriminated-unions
    let allotmentGroupedPerCoach : AllotmentStrategy =
        fun request plan ->
            let coachesCapacity = calculateCoachesCapacity plan

            let firstAvailable =
                coachesCapacity
                |> List.map (asCoachAvailability request.SeatCount)
                |> List.choose availableCoach
                |> List.tryHead

            match firstAvailable with
            | None -> Error(NoCoachAvailable(request))
            | Some available ->
                plan.Seats
                |> availableSeatsForCoach available.Coach
                |> List.take request.SeatCount.Value
                |> Ok
