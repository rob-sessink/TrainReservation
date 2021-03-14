namespace TrainReservation.Tests

module ApiTypes =

    open TrainReservation.ApiTypes
    open TrainReservation.ApplicationTime
    open TrainReservation.TimeProvider
    open Xunit
    open FsUnit.Xunit

    time <- TimeProvider.CurrentFixed()

    [<Fact>]
    let ```test fixed time provider and constructing an API error `` () =
        let err =
            asError time.Now 400 "Failure reserving seats" "/reserve"

        let expected =
            { Message = "Failure reserving seats"
              Status = 400
              Error = "Bad Request"
              Path = "/reserve"
              Timestamp = time.Now.ToString("yyyy-MM-ddTHH:mm:ssK") }

        err |> should equal expected
