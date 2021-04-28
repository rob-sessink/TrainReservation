namespace TrainReservation.TrainDataService

module Adapter =

    open Thoth.Json.Net
    open TrainReservation.Utils
    open TrainReservation.Types
    open TrainReservation.Types.Allocation
    open TrainReservation.Allocation

    /// <summary>Decoder for a map of seat details under fields 'seats'</summary>
    ///
    /// Original Kata Json model did not define the 'reservation_id' field. For the event-sourcing exercise this was
    /// added and to uphold compatibility, if 'reservation_id' field is not defined but a 'booking_reference' value is,
    /// than a ReservationId is generated, otherwise a it defaults to Empty
    ///
    /// https://stackoverflow.com/questions/55746070/how-do-you-deserialize-json-to-a-map-dictionary-of-records-in-f
    /// {
    ///   "seats":
    ///     {
    ///       "1A": { "booking_reference": "", "reservation_id":"", "seat_number": "1", "coach": "A" },
    ///       "2A": { "booking_reference": "2021-02-15-inter_4000-1B", "reservation_id":"1-1-1-1-1", "seat_number": "2", "coach": "A" }
    ///     }
    /// }
    let seatDetailDecoder : Decoder<SeatDetail> =
        Decode.object
            (fun get ->
                let coach = get.Required.Field "coach" Decode.string

                let seatNumber = get.Required.Field "seat_number" Decode.string

                let bookingReference = BookingReference.Create(get.Required.Field "booking_reference" Decode.string)

                let reservationId =
                    match (get.Optional.Field "reservation_id" Decode.string) with
                    | Some guid -> ReservationId.Create(guid)
                    | None -> if bookingReference.Exists then ReservationId.New else ReservationId.Empty

                { Coach = CoachId coach
                  SeatNumber = seatNumber
                  ReservationId = reservationId
                  BookingReference = bookingReference })

    let seatsMapDecoder : Decoder<Seat list> =
        fun path value ->
            let seatsPairs = Decode.keyValuePairs seatDetailDecoder path value

            seatsPairs
            |> Result.map
                (fun sl ->
                    sl
                    |> List.map
                        (fun (seatId, seatDetail) ->
                            { SeatId = SeatId seatId
                              SeatDetail = seatDetail }))

    /// <summary>Decoder for the field 'seats' and underlying map with seat details</summary>
    let (seatsFieldDecoder: string -> JsonValue -> Result<Seat list, DecoderError>) =
        Decode.object (fun get -> get.Required.Field "seats" seatsMapDecoder)

    /// <summary>Helper function to decode the seats json object into</summary>
    /// <param name="json">to decode</param>
    /// <returns>list of seats</returns>
    let decodeSeats json =
        Decode.fromString seatsFieldDecoder json
        |> Result.map (fun seats -> seats |> List.toSeq)

    /// <summary>Decoder for a json map containing train information. Map key is the train identifier. The way the
    /// Json schema was modelled in the original Kata with the key/value pairs is for me actually a no-go!</summary>
    ///{
    ///  "local_1000": {
    ///    "seats": {
    ///      "1A": {
    ///        "coach": "A",
    ///        "seat_number": "1",
    ///        "booking_reference": ""
    ///       },
    ///       .....
    /// }
    let trainsDecoder : Decoder<TrainPlan list> =
        fun path value ->
            let trainPairs = Decode.keyValuePairs seatsFieldDecoder path value

            trainPairs
            |> Result.map
                (fun tl ->
                    tl
                    |> List.map
                        (fun (trainId, seatsList) ->
                            { TrainId = TrainId trainId
                              Seats = seatsList
                              AllocationSettings = defaultAllocationSettings }))

    /// <summary>Helper function to decode the seats json object</summary>
    /// <param name="json">to decode</param>
    /// <returns>list of train information</returns>
    let decodeTrains json = Decode.fromString trainsDecoder json

    let filterTrain trainId (trains: TrainPlan list) =
        trains |> List.filter (fun ti -> ti.TrainId = trainId)

    /// <summary>Provide seating details for a train</summary>
    /// <param name="url">of service endpoint</param>
    /// <param name="request">request to retrieve train details for</param>
    let provideTrainSeatingInformation url : ProvideTrainSeatingInformation =
        fun request ->

            // when using an external service an API request would be made
            let json = readFile url

            match decodeTrains json with
            | Error e -> Error(InvalidTrainPlan $"Failure retrieving train information: {e}")
            | Ok trains ->
                match filterTrain request.TrainId trains with
                | [] ->
                    Error(TrainIdNotFound(request, $"Train information for train: {request.TrainId.Value} not found"))
                | x :: _ -> Ok x


    /// <summary>Update seating details for a train based on a reservation</summary>
    /// <param name="url">of service endpoint</param>
    /// <param name="reservation">used in updating the seating details</param>
    let updateTrainSeatingInformation url : UpdateTrainSeatingInformation = fun reservation -> Ok reservation
