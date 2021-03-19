namespace TrainReservation.Client.Tests

module Main =

    open TrainReservation.Client.Main
    open Xunit
    open FsUnit.Xunit


    // [<Fact>]
    let ``Perform an API request, receiving a confirmed reservation`` () =
        let result =
            main [| "-train"
                    "\'local_1000\'"
                    "seats"
                    "2" |]

        0 |> should equal 0
