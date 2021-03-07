module TrainReservation.Types

/// ---------------------------------------------------------------------------
// Operators
let (>>=) x fn = Result.bind fn x

let (<!>) x fn = Result.map fn x

/// ---------------------------------------------------------------------------
/// Core Domain Types

/// ---------------------------------------------------------------------------
/// Train and Seating Types offered by the train data provider
///

// Train Identifier
type TrainId =
    | TrainId of string
    member this.Value = this |> fun (TrainId id) -> id

// Seat Identifier
type SeatId =
    | SeatId of string
    member this.Value = this |> fun (SeatId id) -> id

// Booking Identifier
type BookingId =
    | BookingId of string
    member this.Value = this |> fun (BookingId id) -> id

/// Individual seat and reservation details
type SeatDetail =
    { Coach: string
      SeatNumber: string
      BookingReference: string } // TODO prefered to use Option.None as indicating of available seat?

/// Seating details
type Seat =
    { SeatId: SeatId
      SeatDetail: SeatDetail }

// Type containing all seating information for a train as provided via the TrainDataService. The information is used
// in seating allotment and reservations
type TrainInformation = { TrainId: TrainId; Seats: Seat list }

/// ---------------------------------------------------------------------------
/// Allocation, Capacity and Reservation Types
type Percentage =
    | Percentage of decimal
    static member value(Percentage id) = id

// Type containing the current, allowed and allocatable capacity of an object in percentages and units
type Capacity =
    { Current: Percentage // current capacity as percentage of total
      MaximumAllowed: Percentage // allowed capacity as percentage of total
      Allocatable: Percentage // allocatable capacity as percentage until maximum
      UnitAllocatable: int // allocatable units
      UnitTotal: int } // total number units

// Type describing the various option of Availability as Capacity for a train, coach or compartment
type Availability =
    | Available of Capacity
    | Unavailable of Capacity
    | MaximumReached of Capacity

// Strategies how seats can be allotted on a train
type AllotmentStrategy =
    | Sequential
    | GroupedPerCoach

// Type holding the settings used in allotment of seats on a train
type AllocationSettings =
    { AllowedCapacity: Percentage
      AllowedCoachCapacity: Percentage
      Allotment: AllotmentStrategy }

// Current Capacity of a Coach
type CoachCapacity = { Coach: string; Capacity: Capacity }

// An allocation of seats on a train from which a confirmed reservation is created
type SeatAllocation = { TrainId: TrainId; Seats: Seat list }

// Confirmed reservation of seats on a train. This confirmation is communicated back to the customer
type ConfirmedReservation =
    { TrainId: TrainId
      BookingId: BookingId
      Seats: Seat list }

// States of a Reservation
type Reservation =
    | Allocated of SeatAllocation
    | Confirmed of ConfirmedReservation

// Events
//type ReservationEvent = ConfirmedReservation of Reservation

/// ---------------------------------------------------------------------------
/// Command Types

// Unvalidated request for reservation received from a customer. Defined using primitive data-types
type UnvalidatedReservationRequest = { TrainId: string; SeatCount: int }

// Validated request for reservation to-be fulfilled by the ticket office
type ValidReservationRequest = { TrainId: TrainId; SeatCount: int }

type ReservationError =
    | InvalidRequest of message: string
    | InvalidTrainId of UnvalidatedReservationRequest * message: string
    | InvalidSeatCount of UnvalidatedReservationRequest * message: string
    | TrainIdNotFound of ValidReservationRequest * message: string
    | InvalidTrainInformation of message: string
    //| BookingServiceUnavailable of string
    //| TrainDataServiceUnavailable of string
    | NoSeatsAvailable of ValidReservationRequest * trainCapacity: Availability
    | NoCoachAvailable of ValidReservationRequest // could pass along trainCapacity
    | MaximumCapacityReached of ValidReservationRequest * trainCapacity: Availability

// Unvalidated request to reset all reservations for a train, as received from an operator
type UnvalidatedResetReservationsRequest = { TrainId: string }

// Validated request to reset all reservations for a train to-be executed by the ticket office
type ValidResetReservationsRequest = { TrainId: TrainId }

/// ---------------------------------------------------------------------------
/// Core Flows offered by the Application

// Reservation Flow (Driven Port)
type ReserveSeatsFlow = UnvalidatedReservationRequest -> Result<ConfirmedReservation, ReservationError>

// Core Reset Reservation Flow (Driven Port)
type ResetReservationsFlow = UnvalidatedResetReservationsRequest -> unit

/// ---------------------------------------------------------------------------
/// Core Ports used by the Application

// Train Information Port (Driving)
type ProvideTrainSeatingInformation = ValidReservationRequest -> Result<TrainInformation, ReservationError>

// Train Information Port (Driving)
type UpdateTrainSeatingInformation = ConfirmedReservation -> Result<ConfirmedReservation, ReservationError>

// Booking Reference Port (Driving)
type ProvideBookingReference = SeatAllocation -> Result<ConfirmedReservation, ReservationError>
