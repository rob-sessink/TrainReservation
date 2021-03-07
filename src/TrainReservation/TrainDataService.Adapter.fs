module TrainReservation.TrainDataService.Adapter

open Thoth.Json.Net
open TrainReservation.Utils
open TrainReservation.Types

/// <summary>Decoder for a map of seat details under fields 'seats'</summary>
///
/// https://stackoverflow.com/questions/55746070/how-do-you-deserialize-json-to-a-map-dictionary-of-records-in-f
/// {
///   "seats":
///     {
///       "1A": { "booking_reference": "", "seat_number": "1", "coach": "A" },
///       "2A": { "booking_reference": "", "seat_number": "2", "coach": "A" }
///     }
/// }
let seatDetailDecoder: Decoder<SeatDetail> =
    Decode.object (fun get ->
        { Coach = get.Required.Field "coach" Decode.string
          SeatNumber = get.Required.Field "seat_number" Decode.string
          BookingReference = get.Required.Field "booking_reference" Decode.string })

let seatsMapDecoder: Decoder<Seat list> =
    fun path value ->
        let seatsPairs =
            Decode.keyValuePairs seatDetailDecoder path value

        seatsPairs
        |> Result.map (fun sl ->
            sl
            |> List.map (fun (seatId, seatDetail) ->
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

/// <summary>Decoder for a json map containing train information. Map key is the train identifier</summary>
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
let trainsDecoder: Decoder<TrainInformation list> =
    fun path value ->
        let trainPairs =
            Decode.keyValuePairs seatsFieldDecoder path value

        trainPairs
        |> Result.map (fun tl ->
            tl
            |> List.map (fun (trainId, seatsList) ->
                { TrainId = TrainId trainId
                  Seats = seatsList }))

/// <summary>Helper function to decode the seats json object</summary>
/// <param name="json">to decode</param>
/// <returns>list of train information</returns>
let decodeTrains json = Decode.fromString trainsDecoder json

let filterTrain trainId (trains: TrainInformation list) =
    trains
    |> List.filter (fun ti -> ti.TrainId = trainId)

/// <summary>Provide seating details for a train</summary>
/// <param name="url">of service endpoint</param>
/// <param name="request">request to retrieve train details for</param>
let provideTrainSeatingInformation url: ProvideTrainSeatingInformation =
    fun request ->

        // normally an API request is done invoked
        let json = readFile url

        match decodeTrains json with
        | Error e -> Error(InvalidTrainInformation $"Failure retrieving train information: {e}")
        | Ok trains ->
            match filterTrain request.TrainId trains with
            | [] -> Error(TrainIdNotFound(request, $"Train information for train: {request.TrainId.Value} was not found"))
            | x :: _ -> Ok x


/// <summary>Update seating details for a train based on a reservation</summary>
/// <param name="url">of service endpoint</param>
/// <param name="reservation">used in updating the seating details</param>
let updateTrainSeatingInformation url: UpdateTrainSeatingInformation = fun reservation -> Ok reservation
