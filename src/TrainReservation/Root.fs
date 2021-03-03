module TrainReservation.Root

open TrainReservation.Types
open TrainReservation.TicketOffice.Controller
open TrainReservation.ReserveSeatsFlow
open TrainReservation.ResetReservationFlow
open TrainReservation.TrainDataService.Adapter
open TrainReservation.BookingService.Adapter
open Giraffe

// ---------------------------------------------------------------------------
/// Composite Root
///
let ComposeReserveSeatsFlow: ReserveSeatsFlow =
    let io =
        { ProvideTrainSeatingInformation = provideTrainSeatingInformation "data/trains.json"
          ProvideBookingReference = bookingReferenceService "http://localhost:8082/booking_reference"
          UpdateTrainSeatingInformation = updateTrainSeatingInformation "http://localhost:8081/update" }

    reserveSeats io

let ComposeReservationHandler: HttpHandler = reservationHandler ComposeReserveSeatsFlow

// compose the resetHandler by injecting the 'resetReservations' function fom the ResetReservationFlow
let ComposeResetTrainHandler trainId: HttpHandler = resetHandler resetReservations trainId
