namespace TrainReservation.Tests

open System
open TrainReservation.TrainPlanService
open TrainReservation.Types
open TrainReservation.Types.Allocation
open Xunit
open Xunit.Abstractions
open FsUnit.Xunit

open TrainPlanFixtures
open Infrastructure

module TrainPlanService =

    module MemoryStore =
        let createService logger store =
            let resolve =
                Equinox
                    .MemoryStore
                    .Resolver(
                        store,
                        Events.codec,
                        Fold.fold,
                        Fold.initial
                    )
                    .Resolve

            create logger resolve


    type TrainPlanServiceTests(outputHelper: ITestOutputHelper) =

        // Fixtures
        let basePlan = TrainPlan.Create "local_1000" seats2_0Pct default_allocation_settings
        let allocatedPlan = TrainPlan.Create "local_1000" seats3_66Pct default_allocation_settings
        let emptyPlan = TrainPlan.Create "local_1000" [] default_allocation_settings

        let reservationId1 = ReservationId.With(Guid("11111111-1111-1111-1111-111111111111"))
        let reservationId2 = ReservationId.With(Guid("11111111-1111-1111-1111-111111111112"))

        let requestFor1 = AllocationRequest.Create "local_1000" 1 reservationId1
        let requestFor3 = AllocationRequest.Create "local_1000" 3 reservationId2
        let cancellationFor1 = AllocationCancellation.Create "local_1000" reservationId1

        let setupStore =
            let createLog () =
                createLogger (TestOutputAdapter outputHelper)

            let createMemoryStore () = Equinox.MemoryStore.VolatileStore()
            let logger, store = createLog (), createMemoryStore ()

            let service = MemoryStore.createService logger store
            service

        [<Fact>]
        let ``Register a TrainPlan -> success and Query registered`` () =
            async {
                let service = setupStore
                do! service.RegisterTrainPlan basePlan

                let! registered = service.QueryTrainPlan basePlan.TrainId
                registered |> should equal basePlan
            }

        [<Fact>]
        let ``Register a train-plan pre-allocated seats -> fail with PendingAllocationExist exception`` () =
            async {
                let service = setupStore

                try
                    do! service.RegisterTrainPlan basePlan
                    let! _ = service.RequestAllocationUntil requestFor1
                    do! service.RegisterTrainPlan basePlan
                    failwith "Expected AllocationException(PendingAllocationExist)"
                with
                | AllocationException (PendingAllocationExist _) -> ()
                | exn -> failwith $"Unexpected exception thrown {exn.StackTrace}"
            }

        [<Fact>]
        let ``Re-register train-plan when an allocation exists -> fail with InvalidTrainPlan exception`` () =
            async {
                let service = setupStore

                try
                    do! service.RegisterTrainPlan allocatedPlan
                    failwith "Expected AllocationException(InvalidTrainPlan)"
                with
                | AllocationException (InvalidTrainPlan _) -> ()
                | exn -> failwith $"Unexpected exception thrown {exn.StackTrace}"
            }

        [<Fact>]
        let ``Cancel a TrainPlan after initial registration -> success and Queried plan is empty`` () =
            async {
                let service = setupStore

                do! service.RegisterTrainPlan basePlan
                do! service.CancelTrainPlan(TrainId "local_1000")

                let! cleared = service.QueryTrainPlan basePlan.TrainId
                cleared |> should equal emptyPlan
            }

        [<Fact>]
        let ``Request single Seat -> success and return Allocation`` () =
            async {
                let service = setupStore
                do! service.RegisterTrainPlan basePlan

                let! allocated = service.RequestAllocationUntil requestFor1

                allocated |> should haveLength 1

                match allocated.Head with
                | Events.SeatsAllocated e -> e.ReservationId |> should equal reservationId1
                | _ -> failwith "Expected SeatsAllocated event"
            }

        [<Fact>]
        let ``Request two seats -> fail with MaximumCapacityReached exception`` () =
            async {
                let service = setupStore

                try
                    do! service.RegisterTrainPlan basePlan
                    let! _ = service.RequestAllocationUntil requestFor3
                    let! _ = service.RequestAllocationUntil requestFor1
                    failwith "Expected AllocationException(MaximumCapacityReached)"
                with
                | AllocationException (MaximumCapacityReached _) -> ()
                | exn -> failwith $"Unexpected exception thrown {exn.StackTrace}"
            }

        [<Fact>]
        let ``Request and than cancel a returned allocation -> success and return Deallocation events`` () =
            async {
                let service = setupStore
                do! service.RegisterTrainPlan basePlan

                let! allocated = service.RequestAllocationUntil requestFor1

                allocated |> should haveLength 1

                match allocated.Head with
                | Events.SeatsAllocated e ->
                    e.Seats.Length |> should equal 1
                    e.ReservationId |> should equal reservationId1
                | _ -> failwith "Expected SeatsAllocated event"

                let! deallocated = service.CancelAllocation cancellationFor1
                deallocated |> should haveLength 1

                match deallocated.Head with
                | Events.SeatsDeallocated e -> e.Seats.Length |> should equal 1
                | _ -> failwith "Expected SeatsDeallocated event"

                match allocated.Head, deallocated.Head with
                | Events.SeatsAllocated a, Events.SeatsDeallocated d ->
                    let aid = a.Seats |> List.map (fun x -> x.SeatId)
                    let did = d.Seats |> List.map (fun x -> x.SeatId)

                    aid |> should equal did // TODO implement unordered comparison of two lists
                | _ -> failwith "Expected SeatsAllocated and SeatsDeallocated events"
            }

        [<Fact>]
        let ``Request allocation without train-plan registered -> fail with UnallocatedTrainPlan exception`` () =
            async {
                let service = setupStore

                try
                    let! _ = service.RequestAllocationUntil requestFor1
                    failwith "Expected AllocationException(UnallocatableTrainPlan)"
                with
                | AllocationException (UnallocatedTrainPlan _) -> ()
                | exn -> failwith $"Unexpected exception thrown {exn.StackTrace}"
            }

        [<Fact>]
        let ``Repeated request with equal ReservationId -> fail with PendingAllocationExist exception`` () =
            async {
                let service = setupStore

                try
                    do! service.RegisterTrainPlan basePlan
                    let! _ = service.RequestAllocationUntil requestFor1
                    let! _ = service.RequestAllocationUntil requestFor1
                    failwith "Expected AllocationException(PendingAllocationExist)"
                with
                | AllocationException (PendingAllocationExist _) -> ()
                | exn -> failwith $"Unexpected exception thrown {exn.StackTrace}"
            }

        [<Fact>]
        let ``Cancel without an allocation -> fail with NoExistingAllocation exception`` () =
            async {
                let service = setupStore

                try
                    do! service.RegisterTrainPlan basePlan
                    let! _ = service.CancelAllocation cancellationFor1
                    failwith "Expected AllocationException(NoExistingAllocation)"
                with
                | AllocationException (MissingAllocation _) -> ()
                | exn -> failwith $"Unexpected exception thrown {exn.StackTrace}"
            }

        [<Fact>]
        let ``Request seats and cancel train-plan -> success`` () =
            async {
                let service = setupStore
                do! service.RegisterTrainPlan basePlan

                let! _ = service.RequestAllocationUntil requestFor3
                do! service.CancelTrainPlan(TrainId "local_1000")
            }

        [<Fact>]
        let ``Cancel train-plan without plan registered -> success`` () =
            async {
                let service = setupStore

                try
                    let! _ = service.CancelTrainPlan(TrainId "local_1000")
                    failwith "Expected AllocationException(UnallocatedTrainPlan)"
                with
                | AllocationException (UnallocatedTrainPlan _) -> ()
                | exn -> failwith $"Unexpected exception thrown {exn.StackTrace}"
            }
