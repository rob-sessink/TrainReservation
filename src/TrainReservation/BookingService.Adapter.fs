namespace TrainReservation.BookingService

module Adapter =

    open TrainReservation.ApplicationTime
    open TrainReservation.Types
    open TrainReservation.Types.Allocation

    type AsConfirmedReservation = BookingId -> Allocation -> Reservation

    /// <summary>Construct a confirmed reservation.</summary>
    /// <param name="bookingId">uniquely identifying the reservation</param>
    /// <param name="seatAllocation">within the train</param>
    /// <returns>ConfirmedReservation</returns>
    let asConfirmedReservation : AsConfirmedReservation =
        fun bookingId seatAllocation ->

            // copy and update bookingId into list [seats].SeatDetail.BookingReference
            let withBookingId (bookingId: BookingId) (seats: Seat list) =
                seats |> List.map (fun seat -> seat.WithBookingId bookingId)

            { TrainId = seatAllocation.TrainId
              BookingId = bookingId
              Seats = withBookingId bookingId seatAllocation.Seats }

    ///<summary>Service to provide booking references for reservation. Dummy implementation of the booking reference
    /// adapter</summary>
    /// <param name="url">of service endpoint</param>
    /// <param name="seatAllocation">for which a booking reference should be provided</param>
    /// <returns>ConfirmedReservation</returns>
    let bookingReferenceService url : ProvideBookingReference =
        fun seatAllocation ->

            // construct a booking reference in format: [date]-[s1]-[s2]-[sX]
            let reference =
                seatAllocation.Seats
                |> List.map (fun s -> s.SeatId.Value)
                |> List.reduce (fun r s -> r + "-" + s)

            let date = time.Now.ToString "yyyy-MM-dd"

            let bookingReference = date + "-" + seatAllocation.TrainId.Value + "-" + reference

            Ok(asConfirmedReservation (BookingId bookingReference) seatAllocation)
