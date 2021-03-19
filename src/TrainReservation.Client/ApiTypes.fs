namespace TrainReservation.Client

/// ---------------------------------------------------------------------------
/// Public API Types used by the client
///
module ApiTypes =

    type ClientReservationRequest = { trainId: string; seats: int }
