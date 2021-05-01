namespace TrainReservation

open System

/// ---------------------------------------------------------------------------
/// Core Domain Types
///
module Types =

    /// ---------------------------------------------------------------------------
    // Operators
    let (>>=) x fn = Result.bind fn x

    let (<!>) x fn = Result.map fn x

    /// ---------------------------------------------------------------------------
    /// Common types

    /// Identifier of a train on which seats can be booked
    type TrainId =
        | TrainId of string

        static member Create(id: string) =
            TrainId
            <| match id with
               | null -> invalidArg "trainId" "trainId is null"
               | t when t.Length < 5 -> invalidArg "trainId" "Train identifier is invalid"
               | t -> t

        member this.Value = this |> fun (TrainId id) -> id

    type SeatCount =
        | SeatCount of int

        static member Create count =
            SeatCount
            <| match count with
               | c when c < 0 -> invalidArg "count" "Seat count cannot be negative"
               | c when c = 0 -> invalidArg "count" "Seat count cannot be zero"
               | _ -> count

        member this.Value = this |> fun (SeatCount count) -> count

    /// Identifier of an individual seat on a coach of a train
    type SeatId =
        | SeatId of string
        member this.Value = this |> fun (SeatId id) -> id

    /// Identifier of a coach of a train
    type CoachId =
        | CoachId of string
        member this.Value = this |> fun (CoachId id) -> id

    /// Booking Identifier
    type BookingId =
        | BookingId of string
        member this.Value = this |> fun (BookingId id) -> id

    /// Booking Reference acts as the identifier for a confirmed booked seat
    type BookingReference =
        | BookingReference of string option
        static member Create id =
            BookingReference
            <| match id with
               | null -> None
               | "" -> None
               | r -> Some r

        static member Empty = BookingReference None

        member this.Exists = this |> fun (BookingReference id) -> id.IsSome

        member this.Value = this |> fun (BookingReference id) -> id

    /// Reservation Identifier acts as the identifier when requesting a seat reservation or a seat is allocated
    type ReservationId =
        | ReservationId of Guid option

        static member New = ReservationId(Some(Guid.NewGuid()))
        static member With guid = ReservationId(Some guid)

        static member Create str =
            ReservationId
            <| match str with
               | null -> None
               | "" -> None
               | r -> Some(Guid(r))

        static member Empty = ReservationId None

        member this.Exists = this |> fun (ReservationId id) -> id.IsSome

        member this.Value = this |> fun (ReservationId guid) -> guid

    /// Seat number, location, reservation and booking details
    type SeatDetail =
        { Coach: CoachId
          SeatNumber: string
          ReservationId: ReservationId
          BookingReference: BookingReference }

        /// copy and update BookingReference
        member this.WithBookingReference reference =
            { this with
                  BookingReference = (BookingReference.Create reference) }

        /// copy and update ReservationId
        member this.WithReservationId id = { this with ReservationId = id }

    /// A allocatable and bookable seat on a coach for a train-plan
    type Seat =
        { SeatId: SeatId
          SeatDetail: SeatDetail }

        // copy and update BookingReference into SeatDetail
        member this.WithBookingId(bookingId: BookingId) =
            { this with
                  SeatDetail = this.SeatDetail.WithBookingReference bookingId.Value }

        // copy and update ReservationId into SeatDetail
        member this.WithReservationId(reservationId: ReservationId) =
            { this with
                  SeatDetail = this.SeatDetail.WithReservationId reservationId }


    module Allocation =

        /// ---------------------------------------------------------------------------
        /// Allocation, Capacity and Reservation Types
        type Percentage =
            | Percentage of decimal
            member this.Value = this |> fun (Percentage id) -> id

        /// Type containing the current, allowed and allocatable capacity of an object in percentages and units
        type Capacity =
            { Current: Percentage // current capacity as percentage of total
              MaximumAllowed: Percentage // allowed capacity as percentage of total
              Allocatable: Percentage // allocatable capacity as percentage until maximum
              UnitAllocatable: int // allocatable units
              UnitTotal: int } // total number units

        /// Type describing the options of Availability as Capacity for a train, coach or compartment
        type Availability =
            | Available of Capacity
            | Unavailable of Capacity
            | MaximumReached of Capacity

        /// Strategies how seats can be allotted on a train
        type AllotmentStrategy =
            | Sequential
            | GroupedPerCoach

        /// Type holding the settings used in allotment of seats on a train
        type AllocationSettings =
            { AllowedCapacity: Percentage
              AllowedCoachCapacity: Percentage
              Allotment: AllotmentStrategy }

        /// <summary>Default allocation settings</summary>
        let defaultAllocationSettings =
            { AllowedCapacity = Percentage 70m
              AllowedCoachCapacity = Percentage 100m
              Allotment = GroupedPerCoach }

        /// Capacity of an individual coach
        type CoachCapacity = { Coach: CoachId; Capacity: Capacity }

        /// Availability of an individual coach
        type CoachAvailability =
            { Coach: CoachId
              Availability: Availability }

        /// ---------------------------------------------------------------------------
        /// Command, Request and Query types

        /// Plan holding all seating and plan information of a train-ride. Used for seat allotment and allocation
        type TrainPlan =
            { TrainId: TrainId
              Seats: Seat list
              AllocationSettings: AllocationSettings }

            static member Create trainId seats settings =
                { TrainId = TrainId trainId
                  Seats = seats
                  AllocationSettings = settings }

            member this.IsAllocatable = this.Seats <> []

            member this.SeatsByReservationId id =
                this.Seats |> List.filter (fun x -> x.SeatDetail.ReservationId = id)

            member this.HasAllocatedSeatsByReservationId id =
                this.Seats |> List.exists (fun x -> x.SeatDetail.ReservationId = id)

            member this.HasAllocatedSeats = this.Seats |> List.exists (fun x -> x.SeatDetail.ReservationId.Exists)

        /// Command to cancel a train plan
        type TrainPlanCancellation =
            { TrainId: TrainId }

            static member Create trainId = { TrainId = TrainId trainId }

        /// copy and update reservationId into list [seats].SeatDetail.ReservationId
        let withReservationId reservationId (seats: Seat list) =
            seats |> List.map (fun seat -> seat.WithReservationId reservationId)

        /// Allocation of seats on a train; referenced via a ReservationId
        type Allocation =
            { TrainId: TrainId
              Seats: Seat list
              ReservationId: ReservationId }

            static member Create trainId seats reservationId =
                { TrainId = TrainId trainId
                  Seats = seats
                  ReservationId = reservationId }

            static member CreateForAllocated trainId reservationId seats =
                { TrainId = trainId
                  Seats = withReservationId reservationId seats
                  ReservationId = reservationId }

        /// De-allocation of seats on a train; referenced via a ReservationId
        type Deallocation =
            { TrainId: TrainId
              Seats: Seat list }

            static member Create trainId seats =
                { TrainId = TrainId trainId
                  Seats = seats }

            static member CreateFromCancelled trainId cancelled =
                { TrainId = trainId
                  Seats = withReservationId ReservationId.Empty cancelled }


        /// Command used to request an allocation of seats on a train.
        /// ReservationId acts as the reference between the reservation and an allocation
        type AllocationRequest =
            { TrainId: TrainId
              SeatCount: SeatCount
              ReservationId: ReservationId }

            static member Create trainId seatCount reservationId =
                { TrainId = TrainId.Create trainId
                  SeatCount = SeatCount.Create seatCount
                  ReservationId = reservationId }

        /// Command used to cancel an existing allocation
        type AllocationCancellation =
            { TrainId: TrainId
              ReservationId: ReservationId }

            static member Create trainId reservationId =
                { TrainId = TrainId trainId
                  ReservationId = reservationId }

        /// Unvalidated request for reservation received from a customer. Defined using primitive data-types
        type UnvalidatedReservationRequest = { TrainId: string; SeatCount: int }

        /// Unvalidated request to reset all reservations for a train-plan as received from an operator
        type UnvalidatedResetReservationsRequest = { TrainId: string }

        type AllocationError =
            | InvalidRequest of message: string
            | TrainIdNotFound of AllocationRequest * message: string
            | InvalidTrainPlan of message: string
            | UnallocatedTrainPlan of message: string
            | PendingAllocationExist of message: string
            | MissingAllocation of message: string
            | NoSeatsAvailable of AllocationRequest * trainCapacity: Availability
            | NoCoachAvailable of AllocationRequest
            | MaximumCapacityReached of AllocationRequest * trainCapacity: Availability

        // Exception indicating that the allocation allocation process initiated by an command or query failed
        exception AllocationException of AllocationError

        /// Confirmed reservation of seats for a train-plan. This confirmation is communicated back to the customer
        type Reservation =
            { TrainId: TrainId
              // ReservationId: ReservationId
              BookingId: BookingId
              Seats: Seat list }

        /// ---------------------------------------------------------------------------
        /// Core Flows offered by the Application

        /// Reservation Flow (Driven Port)
        type ReserveSeatsFlow = UnvalidatedReservationRequest -> Result<Reservation, AllocationError>

        /// Core Reset Reservation Flow (Driven Port)
        type ResetReservationsFlow = UnvalidatedResetReservationsRequest -> unit

        /// ---------------------------------------------------------------------------
        /// Core Ports used by the Application

        /// Train Information Port (Driving)
        type ProvideTrainSeatingInformation = AllocationRequest -> Result<TrainPlan, AllocationError>

        /// Train Information Port (Driving)
        type UpdateTrainSeatingInformation = Reservation -> Result<Reservation, AllocationError>

        /// Booking Reference Port (Driving)
        type ProvideBookingReference = Allocation -> Result<Reservation, AllocationError>
