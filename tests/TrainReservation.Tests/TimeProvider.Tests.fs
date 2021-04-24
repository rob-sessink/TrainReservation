namespace TrainReservation.Tests

module TimeProvider =

    open TrainReservation
    open TimeProvider
    open System
    open Xunit
    open FsUnit.Xunit

    [<Fact>]
    let ``acquire fixed current time via TimeProvider`` () =

        let currentFixed = TimeProvider.CurrentFixed()

        let time1 = currentFixed.Now
        let time2 = currentFixed.Now

        time1 |> should equal time2

    [<Fact>]
    let ``acquire system time via TimeProvider`` () =

        let systemTime = TimeProvider.SystemTime()

        let time1 = systemTime.Now
        let time2 = systemTime.Now

        time2 |> should greaterThan time1

    [<Fact>]
    let ``acquire a fixed time via TimeProvider`` () =
        let localNow = DateTimeOffset.Now

        let fixedTime = TimeProvider.By(FixedTimeProvider.From(localNow))

        let time1 = fixedTime.Now

        localNow |> should equal time1
