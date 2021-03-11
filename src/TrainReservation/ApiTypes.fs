namespace TrainReservation

/// ---------------------------------------------------------------------------
/// Public API Types used by a client
///
module ApiTypes =

    type ClientReservationRequest = { trainId: string; seats: int }

    type ClientResetReservationRequest = { trainId: string }

    module Error =

        open Giraffe
        open Microsoft.AspNetCore.WebUtilities

        type Error =
            { Message: string
              Status: int
              Error: string
              Path: string
              Timestamp: string }

        let build statusCode message path =
            let err =
                { Message = message
                  Status = statusCode
                  Error = ReasonPhrases.GetReasonPhrase(statusCode)
                  Path = path
                  Timestamp = System.DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssK") }

            setStatusCode statusCode >=> (negotiate err)
