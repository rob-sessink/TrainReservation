namespace TrainReservation

open TrainReservation.Types
open TrainReservation.Types.Allocation

/// TrainPlan aggregate represents planned train rides for which seats can be allocated or deallocated.
///
/// State:
/// DC-1. state currently hold simple properties used in allocation, in a more elaborate engine, allocation could also be
/// done on properties like trip-details, fairs, seating-options etc.
///
/// TrainId:
/// DC-2. TrainId was not part of the state, would be handy for handling but would become optional .
///
/// Capacity:
/// 1. In processing an allocation the capacity of the train is re-calculated based on the current train plan and
/// allocation settings. (calculated capacity is not stored as part of the state and not published via events)
/// 2. Current capacity can be queried via the service after which it is re-calculated based on current plan state
///
/// TrainPlan
/// IV-G1: register train-plan: ensure no allocations are pending on an existing train-plan.
/// IV-G2: register train-plan: ensure no train plan with pre-allocated seats is registered.
///
/// AllocationRequest:
/// IV-G3: train-plan: ensure a train plan is registered and allocatable.
/// IV-G4: request: ensure an reservationId is not already used for another allocation.
///
/// AllocationCancellation:
/// IV-G5: cancellation: ensure a reservationId is actually used in an allocation.
///
/// TD1: train plan reaching it's maximum capacity is actually an business event which must explicitly emitted.
/// TD2: temporal part of an allocation is not handled, requires a periodical check and free-up of allocations
/// TD3: extend event model with metadata: id, version, datetime etc.
/// TD4: implement idempotency check on command interpretation
///
module TrainPlanService =

    // Discriminator in order to allow each train-plan to maintain independent state
    let (|ForTrainId|) (id: TrainId) =
        FsCodec.StreamName.create "TrainPlanService" id.Value

    [<RequireQualifiedAccess>]
    module Events =

        type Event =
            | TrainPlanAdded of TrainPlan
            | TrainPlanCancelled of TrainPlanCancellation
            | SeatsAllocated of Allocation
            | SeatsDeallocated of Deallocation
            interface TypeShape.UnionContract.IUnionContract

        let codec = FsCodec.NewtonsoftJson.Codec.Create<Event>()

    module Fold =

        open ListUtil

        type State =
            { Seats: Seat list
              AllocationSettings: AllocationSettings
              NextId: int }

            /// update every entry matching a changed entries through seatId
            member private this.UpdateSeats changed =
                let matchSeatId x = (fun y -> y.SeatId = x.SeatId)

                changed
                |> List.fold (fun state x -> (update_element (matchSeatId x) x state)) this.Seats

            member private this.EvolveSeats(seats: Seat list) =
                { Seats = seats
                  AllocationSettings = this.AllocationSettings
                  NextId = this.NextId + 1 }

            /// convert state & id to train plan
            member internal this.ToTrainPlan trainId =
                { TrainId = trainId
                  Seats = this.Seats
                  AllocationSettings = this.AllocationSettings }

            member internal this.ClearTrainPlan() = { this with Seats = [] }

            member internal this.EvolveTrainPlan(plan: TrainPlan) = this.EvolveSeats(plan.Seats)

            member internal this.EvolveAllocation(allocation: Allocation) =
                this.EvolveSeats(this.UpdateSeats allocation.Seats)

            member internal this.EvolveDeallocation(deallocation: Deallocation) =
                this.EvolveSeats(this.UpdateSeats deallocation.Seats)

        /// initial state
        let initial =
            { Seats = []
              AllocationSettings = Allocation.defaultAllocationSettings
              NextId = 0 }

        /// evolve state by handling events
        let private evolve (state: State) (event: Events.Event) =
            match event with
            | Events.TrainPlanAdded plan -> state.EvolveTrainPlan plan
            | Events.TrainPlanCancelled _ -> state.ClearTrainPlan()
            | Events.SeatsAllocated allocated -> state.EvolveAllocation allocated
            | Events.SeatsDeallocated deallocated -> state.EvolveDeallocation deallocated

        let fold : State -> Events.Event seq -> State = Seq.fold evolve

    open TrainReservation.Allocation

    type Command =
        | RegisterTrainPlan of TrainPlan
        | CancelTrainPlan of TrainId
        | RequestAllocationUntil of AllocationRequest
        | CancelAllocation of AllocationCancellation

    let private interpret cmd (state: Fold.State) =
        match cmd with
        | RegisterTrainPlan plan ->
            let current = state.ToTrainPlan plan.TrainId
            let registered = registerPlan plan current

            [ Events.TrainPlanAdded registered ]
        | CancelTrainPlan trainId ->
            let current = state.ToTrainPlan trainId
            let deallocated = current |> cancelAllAllocationsForPlan |> List.map Events.SeatsDeallocated
            Events.TrainPlanCancelled { TrainId = trainId } :: deallocated
        | RequestAllocationUntil request ->
            let current = state.ToTrainPlan request.TrainId
            let allocated = allocateSeats request current

            match allocated with
            | Ok all -> [ Events.SeatsAllocated all ]
            | Error err -> raise (AllocationException err) // split off CapacityError and fire as event
        | CancelAllocation request ->
            let current = state.ToTrainPlan request.TrainId
            let deallocated = cancelAllocation request current

            [ Events.SeatsDeallocated deallocated ]


    type Service internal (log, resolve, maxAttempts) =

        let resolve (ForTrainId streamId) =
            Equinox.Stream(log, resolve streamId, maxAttempts)

        let execute clientId command =
            let decider = resolve clientId
            decider.Transact(interpret command)

        /// <summary>Handle command and return updated state and emitted events</summary>
        /// <param name="trainId">subject to command</param>
        /// <param name="command">to execute</param>
        /// <returns>updated state and emitted events</returns>
        let handle (trainId: TrainId) command =
            let decider = resolve trainId

            decider.Transact
                (fun state ->
                    let events = interpret command state
                    let state' = Fold.fold state events
                    (state', events), events)

        /// <summary>Query a train plan in synchronous manner!</summary>
        /// <param name="train">to query</param>
        /// <returns>TrainPlan</returns>
        member _.QueryTrainPlan(train: TrainId) : Async<TrainPlan> =
            let decider = resolve train

            decider.Query
                (fun state ->
                    { TrainId = train
                      Seats = state.Seats
                      AllocationSettings = state.AllocationSettings })

        /// <summary>Register a train plan</summary>
        /// <param name="trainPlan">to register</param>
        /// <exception>AllocationException if registration failed</exception>
        member _.RegisterTrainPlan(trainPlan: TrainPlan) =
            execute trainPlan.TrainId (RegisterTrainPlan trainPlan)

        /// <summary>Cancel a train plan</summary>
        /// <param name="trainId"> of plan to cancel</param>
        /// <exception>AllocationException if cancellation failed</exception>
        member _.CancelTrainPlan(trainId: TrainId) =
            execute trainId (CancelTrainPlan trainId)

        /// <summary>Request an allocation of seats on train</summary>
        /// <param name="request"> for allocation</param>
        /// <returns>SeatsAllocated event</returns>
        /// <exception>AllocationException if allocation failed</exception>
        member _.RequestAllocationUntil(request: AllocationRequest) : Async<Events.Event list> =
            async {
                let! _, events = handle request.TrainId (RequestAllocationUntil request)
                return events
            }

        /// <summary>Cancel an allocation of seats on train</summary>
        /// <param name="cancellation"> of allocation</param>
        /// <returns>SeatsDeallocated event</returns>
        /// <exception>AllocationException if cancellation failed</exception>
        member _.CancelAllocation(cancellation: AllocationCancellation) : Async<Events.Event list> =
            async {
                let! _, events = handle cancellation.TrainId (CancelAllocation cancellation)
                return events
            }

    // Constructor of domain service
    let create log resolve = Service(log, resolve, maxAttempts = 3)
