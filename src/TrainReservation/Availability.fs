namespace TrainReservation

module Capacity =

    open System
    open TrainReservation.Types

    type CalculateCapacity = Percentage -> int -> int -> Capacity

    ///<summary>Calculate capacity</summary>
    ///<param name="maximumAllowed">allowed capacity as percentage</param>
    ///<param name="unitTotal">total number of allocatable units</param>
    ///<param name="unitAllocated">number of units already allocated</param>
    ///<returns>calculated capacity</returns>
    let calculateCapacity: CalculateCapacity =

        fun maximumAllowed unitTotal unitAllocated ->

            let total = decimal unitTotal
            let allocated = decimal unitAllocated

            // (1.m - (30.m - 20.m) / 30.m) * 100.m = 67.666
            // 67.666 -> rounded down = 67
            let current =
                (1.m - ((total - allocated) / total)) * 100.m
                |> Math.Round

            // 70 - 67 = 3
            let allocatable = maximumAllowed.Value - current

            // 30.m * (3.m / 100.m) -> rounded down = 0
            let unitAllocatable =
                total * (allocatable / 100.m) |> Math.Round |> int

            { Current = current |> Percentage
              MaximumAllowed = maximumAllowed
              Allocatable = allocatable |> Percentage
              UnitAllocatable = unitAllocatable
              UnitTotal = unitTotal }


module Availability =

    open Capacity
    open TrainReservation.Types

    /// <summary>Get all available seats in coach</summary>
    let availableSeatsForCoach coach seats =
        seats
        |> List.filter (fun s ->
            s.SeatDetail.Coach = coach
            && s.SeatDetail.BookingReference = "")

    /// <summary>Count seats allocated in a train</summary>
    let countAllocatedSeats seats =
        seats
        |> List.filter (fun s -> s.SeatDetail.BookingReference <> "")
        |> List.length


    type CalculateCoachesCapacity = AllocationSettings -> TrainInformation -> CoachCapacity list

    /// <summary>Calculate the capacity for every coach in a train</summary>
    /// <param name="information">TrainInformation</param>
    /// <param name="settings">used in determining available capacity</param>
    /// <returns>list of tuples with the capacity per coach</returns>
    let calculateCoachesCapacity: CalculateCoachesCapacity =
        fun settings information ->

            let seatCapacity settings seats =
                let total = seats |> List.length
                let allocated = countAllocatedSeats seats
                calculateCapacity settings.AllowedCoachCapacity total allocated

            let coachesCapacity =
                information.Seats
                |> List.groupBy (fun s -> s.SeatDetail.Coach)
                |> List.map (fun s ->
                    ({ Coach = fst s
                       Capacity = seatCapacity settings (snd s) }))

            coachesCapacity


    type CalculateTrainCapacity = AllocationSettings -> TrainInformation -> Capacity

    /// <summary>Calculate the capacity for train</summary>
    /// <param name="information">TrainInformation</param>
    /// <param name="settings"> </param>
    /// <returns>Capacity</returns>
    let calculateTrainCapacity: CalculateTrainCapacity =
        fun settings information ->

            let total = information.Seats |> List.length
            let allocated = countAllocatedSeats information.Seats
            calculateCapacity settings.AllowedCapacity total allocated


    /// <summary>Determine availability based on capacity and seats requested</summary>
    /// <param name="requested">seats</param>
    /// <param name="capacity">of the train or coach</param>
    /// <returns>Capacity</returns>
    let toAvailability requested capacity: Availability =
        match capacity with
        | c when c.Allocatable > Percentage 0m
                 && c.UnitAllocatable < requested -> Unavailable c
        | c when c.Allocatable > Percentage 0m
                 && c.UnitAllocatable >= requested -> Available c
        | c when c.Allocatable <= Percentage 0m -> MaximumReached c
        | c -> Unavailable c


    type AllotmentStrategy =
        ValidReservationRequest -> AllocationSettings -> TrainInformation -> Result<Seat list, ReservationError>


    /// <summary>Strategy to find a single coach that allows the grouped allotment of the requested seats</summary>
    /// <param name="request">for the reservation of seats</param>
    /// <param name="allocationSettings">train allocation settings</param>
    /// <param name="trainInformation">train seating and occupation details</param>
    /// <returns>allocatable coach </returns>
    /// https://stackoverflow.com/questions/34120591/f-generic-function-filter-list-of-discriminated-unions
    let groupedPerCoachAllotment: AllotmentStrategy =
        fun request allocationSettings trainInformation ->

            let coachesCapacity =
                calculateCoachesCapacity allocationSettings trainInformation

            let tryAssumeAvailable coach =
                match (snd coach) with
                | Available _ -> Some coach
                | _ -> None

            let firstAvailable =
                coachesCapacity
                |> List.map (fun c -> (c.Coach), toAvailability request.SeatCount c.Capacity)
                |> List.choose (tryAssumeAvailable)
                |> List.tryHead

            match firstAvailable with
            | None -> Error(NoCoachAvailable(request))
            | Some coach ->
                trainInformation.Seats
                |> availableSeatsForCoach (fst coach)
                |> List.take request.SeatCount
                |> Ok
