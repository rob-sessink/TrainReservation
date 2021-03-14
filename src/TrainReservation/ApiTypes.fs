namespace TrainReservation

/// ---------------------------------------------------------------------------
/// Public API Types used by a client
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

    let asError (time: DateTimeOffset) statusCode message path =
        { Message = message
          Status = statusCode
          Error = ReasonPhrases.GetReasonPhrase(statusCode)
          Path = path
          Timestamp = time.ToString("yyyy-MM-ddTHH:mm:ssK") }

    let error time statusCode message path =
        let err = asError time statusCode message path
        setStatusCode statusCode >=> (negotiate err)
