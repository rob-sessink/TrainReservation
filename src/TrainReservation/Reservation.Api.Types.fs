module TrainReservation.Reservation.Api.Types

/// ---------------------------------------------------------------------------
/// Public API Types used by a client
type ClientReservationRequest = { trainId: string; seats: int }

type ClientResetReservationRequest = { trainId: string }
