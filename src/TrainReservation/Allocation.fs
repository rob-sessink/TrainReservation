namespace TrainReservation

module Allocation =

    open TrainReservation.Availability
    open TrainReservation.Types


    type ProvideAllocationSettings = TrainInformation -> AllocationSettings

    /// <summary>Provide allocation settings for a train</summary>
    /// <param name="trainInformation">with train seating and occupation details</param>
    /// <returns>allocation settings</returns>
    let provideReservationSettings: ProvideAllocationSettings =
        fun trainInformation ->
            let allocationSettings =
                { AllowedCapacity = Percentage 70m
                  AllowedCoachCapacity = Percentage 100m
                  Allotment = GroupedPerCoach }

            allocationSettings

    // ValidReservationRequest is used as allocated could also be done on various properties of a reservation like
    // requested, date, travel details etc. With more complex allocation strategy first a train would be
    type CapacityStrategy = ValidReservationRequest -> TrainInformation -> Availability

    /// <summary>Allocation strategy based on the capacity of the train and seats to allocate.</summary>
    /// <param name="request">details of the reservation</param>
    /// <param name="trainInformation">train seating and occupation details</param>
    /// <returns>Availability of the train</returns>
    let capacityStrategy: CapacityStrategy =
        fun request trainInformation ->

            let allocationSettings =
                provideReservationSettings trainInformation

            let trainCapacity =
                calculateTrainCapacity allocationSettings trainInformation

            toAvailability request.SeatCount trainCapacity


    type AllocateSeatsInTrain = ValidReservationRequest -> TrainInformation -> Result<SeatAllocation, ReservationError>

    /// <summary>Allocate actual seats in a train for a reservation request</summary>
    /// <param name="request">for reservation of seats</param>
    /// <param name="trainInformation">train seating and occupation details</param>
    /// <returns>allocation of seats</returns>
    let allocateSeatsInTrain: AllocateSeatsInTrain =
        fun request trainInformation ->

            let allocationSettings =
                provideReservationSettings trainInformation

            let allotted =
                match allocationSettings.Allotment with
                | Sequential -> groupedPerCoachAllotment request allocationSettings trainInformation
                | GroupedPerCoach -> groupedPerCoachAllotment request allocationSettings trainInformation

            allotted
            |> Result.map (fun seats ->
                { TrainId = trainInformation.TrainId
                  Seats = seats })


    type AllocateSeats = ValidReservationRequest -> TrainInformation -> Result<SeatAllocation, ReservationError>

    /// <summary>For a reservation request, try to allocate seats. In a real booking engine, capacity and seating would be
    /// calculated based up request and travel details and offered back to the client for selection. This Kata only
    /// supports booking a train up-front by identifier and fixed allocation rules.</summary>
    /// <param name="request">for the reservation of seats</param>
    /// <param name="trainInformation">train seating and occupation details</param>
    /// <returns>allocation of seats</returns>
    let allocateSeats: AllocateSeats =
        fun request trainInformation ->

            let availability =
                capacityStrategy request trainInformation

            match availability with
            | MaximumReached _ -> Error(MaximumCapacityReached(request, availability))
            | Unavailable _ -> Error(NoSeatsAvailable(request, availability))
            | Available _ -> allocateSeatsInTrain request trainInformation
