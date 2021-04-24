namespace TrainReservation

module ResetReservationFlow =

    open TrainReservation.Types.Allocation

    /// <summary>Reset all seat reservation for a train-plan</summary>
    /// <param name="unvalidatedRequest">to reset seat reservation</param>
    let resetReservations : ResetReservationsFlow = fun unvalidatedRequest -> ()
