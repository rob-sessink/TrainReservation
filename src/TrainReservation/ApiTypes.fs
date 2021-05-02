namespace TrainReservation

/// public API Types (defined with pure primitives) as send by a client
///
module ApiTypes =

    open System
    open Giraffe
    open Microsoft.AspNetCore.WebUtilities

    type ClientReservationRequest = { trainId: string; seats: int }

    type ClientResetReservationRequest = { trainId: string }

    type Error =
        { Message: string
          Status: int
          Error: string
          Path: string
          Timestamp: string }

    let asError (time: DateTimeOffset) status message path =
        { Message = message
          Status = status
          Error = ReasonPhrases.GetReasonPhrase(status)
          Path = path
          Timestamp = time.ToString("yyyy-MM-ddTHH:mm:ssK") }

    let error time status message path =
        let err = asError time status message path
        setStatusCode status >=> (negotiate err)
