namespace TrainReservation.Tests.BookingService

open System

module Adapter =

    open Xunit
    open FsUnit.Xunit
    open TrainReservation.BookingService.Adapter
    open TrainReservation.ApplicationTime
    open TrainReservation.TimeProvider
    open TrainReservation.Types
    open TrainReservation.Types.Allocation

    time <- TimeProvider.CurrentFixed()

    /// Fixtures
    let unreserved_a1 =
        { SeatId = SeatId "1A"
          SeatDetail =
              { Coach = "A"
                SeatNumber = "1"
                ReservationId = ReservationId.With(Guid.Empty)
                BookingReference = BookingReference.Empty } }

    let unreserved_a2 =
        { SeatId = SeatId "2A"
          SeatDetail =
              { Coach = "A"
                SeatNumber = "2"
                ReservationId = ReservationId.With(Guid.Empty)
                BookingReference = BookingReference.Empty } }

    let seats_allocated_0 =
        { TrainId = TrainId "local_1000"
          Seats = [ unreserved_a1; unreserved_a2 ]
          ReservationId = ReservationId.With(Guid.Empty) }


    [<Fact>]
    let ``Request a booking reference for a seat allocation`` () =
        let bookingReferenceService = bookingReferenceService "http://localhost:8082"
        let reference = bookingReferenceService seats_allocated_0
        let bookingId = (time.Now.ToString "yyyy-MM-dd") + "-local_1000-1A-2A"

        let expected = asConfirmedReservation (BookingId bookingId) seats_allocated_0

        reference |> Result.map (should equal expected)
