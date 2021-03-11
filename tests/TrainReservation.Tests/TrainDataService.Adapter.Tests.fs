namespace TrainReservation.Tests.TrainDataService

module Adapter =

    open TrainReservation.Tests.Fixtures
    open TrainReservation.TrainDataService.Adapter
    open Xunit

    [<Fact>]
    let ``Decode seats from json object map`` () =
        let json = readFixture "fixtures/seats.json"

        match decodeSeats json with
        | Ok _ -> ()
        | Error e -> failwith e


    [<Fact>]
    let ``Decode trains from json object map`` () =
        let json = readFixture "fixtures/trains.json"

        match decodeTrains json with
        | Ok _ -> ()
        | Error e -> failwith e
