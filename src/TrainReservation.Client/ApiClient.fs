namespace TrainReservation.Client


module ApiClient =

    open Newtonsoft.Json
    open System.Net.Http
    open System.Text
    open TrainReservation.Client.ApiTypes

    let client = new HttpClient()

    let postAsync (client: HttpClient) (url: string) (request: ClientReservationRequest) =
        async {
            try
                let json = JsonConvert.SerializeObject(request)

                use content =
                    new StringContent(json, Encoding.UTF8, "application/json; charset=utf-8")

                let! response = client.PostAsync(url, content) |> Async.AwaitTask

                let! body =
                    response.Content.ReadAsStringAsync()
                    |> Async.AwaitTask

                return
                    match response.IsSuccessStatusCode with
                    | true -> Ok body
                    | false -> Error body
            with :? HttpRequestException as ex -> return Error ex.Message
        }
        |> Async.RunSynchronously
