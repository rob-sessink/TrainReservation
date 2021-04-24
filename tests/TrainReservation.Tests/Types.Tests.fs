namespace TrainReservation.Tests

open System
open TrainReservation.Types
open TrainReservation.Types.Allocation
open Xunit
open FsUnit.Xunit


module Types =

    type ``BookingReference Type Tests``() =

        [<Fact>]
        let ``BookingReference Create empty`` () =
            BookingReference.Empty |> should equal (BookingReference.Create "")

        [<Fact>]
        let ``BookingReference Create null`` () =
            BookingReference.Empty |> should equal (BookingReference.Create null)

        [<Fact>]
        let ``BookingReference Value`` () =
            (BookingReference.Create "1").Value |> should equal (Some "1")

        [<Fact>]
        let ``BookingReference Exists`` () =
            (BookingReference.Create "1").Exists |> should equal true

        [<Fact>]
        let ``BookingReference Empty`` () =
            BookingReference.Empty.Value |> should equal None


    type ``ReservationId Type Tests``() =

        let guid_1 = Guid("11111111-1111-1111-1111-111111111111")

        [<Fact>]
        let ``ReservationId With`` () =
            (ReservationId.With(guid_1)).Value |> should equal (Some guid_1)

        [<Fact>]
        let ``ReservationId Create empty`` () =
            ReservationId.Empty |> should equal (ReservationId.Create "")

        [<Fact>]
        let ``ReservationId Create null`` () =
            ReservationId.Empty |> should equal (ReservationId.Create null)

        [<Fact>]
        let ``ReservationId Create Guid`` () =
            (ReservationId.Create "11111111-1111-1111-1111-111111111111")
                .Value
            |> should equal (Some guid_1)

        [<Fact>]
        let ``ReservationId Create Invalid Guid`` () =
            (fun () -> ReservationId.Create "11111111-1111-1111-1111-" |> ignore)
            |> should throw typeof<FormatException>

        [<Fact>]
        let ``ReservationId New`` () =
            ReservationId.New.Value |> should not' (equal (Some(Guid.Empty)))

        [<Fact>]
        let ``ReservationId Exists`` () =
            ReservationId.New.Exists |> should equal true

        [<Fact>]
        let ``ReservationId Empty`` () =
            ReservationId.Empty.Value |> should equal None


    module Allocation =

        open TrainReservation.Tests.TrainPlanFixtures

        let plan_2s_0pct_default = TrainPlan.Create "local_1000" seats2_0Pct default_allocation_settings
        let plan_3s_66pct_default = TrainPlan.Create "local_1000" seats3_66Pct default_allocation_settings

        type ``TrainPlan Type Tests``() =

            [<Fact>]
            let ``Determine Train Plan has no allocated seats`` () =
                plan_2s_0pct_default.HasAllocatedSeats |> should be False

            [<Fact>]
            let ``Determine Train Plan has allocated seats`` () =
                plan_3s_66pct_default.HasAllocatedSeats |> should be True

            [<Fact>]
            let ``Determine Train Plan has allocated seats by reservationId`` () =
                plan_3s_66pct_default.HasAllocatedSeatsByReservationId reservationId1
                |> should be True

            [<Fact>]
            let ``Allocated seats by reservationId`` () =
                plan_3s_66pct_default.SeatsByReservationId reservationId1
                |> List.length
                |> should equal 1
