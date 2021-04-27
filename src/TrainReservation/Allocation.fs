namespace TrainReservation

module Allocation =

    open TrainReservation.Availability
    open TrainReservation.Types
    open TrainReservation.Types.Allocation

    type ProvideAllocationSettings = TrainPlan -> AllocationSettings

    /// <summary>Default allocation settings</summary>
    let defaultAllocationSettings =
        { AllowedCapacity = Percentage 70m
          AllowedCoachCapacity = Percentage 100m
          Allotment = GroupedPerCoach }

    /// <summary>Provide allocation specific for a train</summary>
    /// <returns>allocation settings</returns>
    let provideAllocationSettings : ProvideAllocationSettings = fun _ -> defaultAllocationSettings

    // ValidReservationRequest is used as allocated could also be done on various properties of a reservation like
    // requested, date, travel details etc. With more complex allocation strategy first a train would be
    type CapacityStrategy = AllocationRequest -> TrainPlan -> Availability

    /// <summary>Allocation strategy based on the capacity of the train and number of seats to allocate.</summary>
    /// <param name="request">for allocation of seats</param>
    /// <param name="plan">train seating and occupation details</param>
    /// <returns>Availability of the train</returns>
    let capacityStrategy : CapacityStrategy =
        fun request plan ->
            let trainCapacity = calculateTrainCapacity plan
            toAvailability request.SeatCount trainCapacity

    type AllocateSeatsInTrain = AllocationRequest -> TrainPlan -> Result<Allocation, AllocationError>

    /// <summary>Allocate actual seats in the train for the allocation request</summary>
    /// <param name="request">for allocation of seats</param>
    /// <param name="plan">train seating and allocation details</param>
    /// <returns>allocation of seats</returns>
    let allocateSeatsInTrain : AllocateSeatsInTrain =
        fun request plan ->
            let allotted =
                match plan.AllocationSettings.Allotment with
                | Sequential -> allotmentGroupedPerCoach request plan
                | GroupedPerCoach -> allotmentGroupedPerCoach request plan

            allotted
            |> Result.map (Allocation.CreateForAllocated plan.TrainId request.ReservationId)


    /// IV-G4: request: ensure an reservationId is not already used for another allocation.
    let ensureReservationIdIsNotAllocated (request: AllocationRequest) (plan: TrainPlan) =
        if plan.HasAllocatedSeatsByReservationId request.ReservationId then
            raise (
                AllocationException(PendingAllocationExist $"ReservationId {request.ReservationId} already allocated")
            )

    /// IV-G3: train-plan: ensure a train plan is registered and allocatable.
    let ensureTrainPlanIsAllocatable (plan: TrainPlan) =
        if not plan.IsAllocatable then
            raise (AllocationException(UnallocatedTrainPlan "No train-plan is registered or allocatable"))

    type AllocateSeats = AllocationRequest -> TrainPlan -> Result<Allocation, AllocationError>

    /// <summary>For a reservation request, try to allocate seats. In a real booking engine, capacity and seating could
    /// be calculated based up request and travel details and possibly offered back as a choice to the client for
    /// selection. This Kata only supports booking a train up-front by identifier and fixed allocation rules.</summary>
    /// <param name="request">for allocation of seats</param>
    /// <param name="plan">train seating and allocation details</param>
    /// <returns>allocated seats or error</returns>
    let allocateSeats : AllocateSeats =
        fun request plan ->
            ensureTrainPlanIsAllocatable plan
            ensureReservationIdIsNotAllocated request plan

            let availability = capacityStrategy request plan

            match availability with
            | MaximumReached _ -> Error(MaximumCapacityReached(request, availability))
            | Unavailable _ -> Error(NoSeatsAvailable(request, availability))
            | Available _ -> allocateSeatsInTrain request plan

    /// IV-G5: cancellation: ensure a reservationId is actually used in an allocation.
    let ensureReservationIdIsAllocated (cancel: AllocationCancellation) (plan: TrainPlan) =
        if not (plan.HasAllocatedSeatsByReservationId cancel.ReservationId) then
            raise (AllocationException(MissingAllocation $"Reservation {cancel.ReservationId} is not allocated"))

    type CancelAllocation = AllocationCancellation -> TrainPlan -> Deallocation

    /// <summary>Cancel an allocation</summary>
    /// <param name="cancellation">of seats</param>
    /// <param name="plan">train seating and allocation details</param>
    /// <returns>de-allocated seats or exception</returns>
    let cancelAllocation : CancelAllocation =
        fun cancellation plan ->
            let cancelled = plan.SeatsByReservationId cancellation.ReservationId
            ensureReservationIdIsAllocated cancellation plan
            cancelled |> Deallocation.CreateFromCancelled plan.TrainId


    /// <summary>Project a train plan to a list of allocation grouped by reservationId</summary>
    /// <param name="plan">to project</param>
    /// <returns>allocation</returns>
    let projectToDeallocationPerReservationId (plan: TrainPlan) =
        plan.Seats
        |> List.groupBy (fun x -> x.SeatDetail.ReservationId)
        |> List.filter (fun (r, _) -> r.Exists)
        |> List.map (fun (_, s) -> { TrainId = plan.TrainId; Seats = s })

    type CancelAllAllocationsForPlan = TrainPlan -> Deallocation list

    /// <summary>Cancel all existing allocation for a train plan</summary>
    /// <param name="plan">to cancel</param>
    /// <returns>cancelled allocations</returns>
    let cancelAllAllocationsForPlan : CancelAllAllocationsForPlan =
        fun plan ->
            ensureTrainPlanIsAllocatable plan
            projectToDeallocationPerReservationId plan


    /// IV-G2: register train-plan: ensure no train plan with pre-allocated seats is registered.
    let validateTrainPlanBeforeRegistration (plan: TrainPlan) =
        if plan.HasAllocatedSeats then
            raise (AllocationException(InvalidTrainPlan "Cannot register train-plan containing pre-allocated seats"))

    /// IV-G1: register train-plan: ensure no allocations are pending on an existing train-plan.
    let ensureNoPendingAllocationsExistFor (existing: TrainPlan) =
        if existing.HasAllocatedSeats then
            raise (AllocationException(PendingAllocationExist "Cannot register train-plan if allocations are pending"))

    type RegisterPlan = TrainPlan -> TrainPlan -> TrainPlan

    /// <summary>Register a train plan</summary>
    /// <param name="plan">to register</param>
    /// <param name="existing">train seating and allocation details</param>
    let registerPlan : RegisterPlan =
        fun plan existing ->
            ensureNoPendingAllocationsExistFor existing
            validateTrainPlanBeforeRegistration plan
            plan
