namespace TrainReservation.Client


module Main =

    open Argu
    open System
    open TrainReservation.Client.Arguments

    let parseResults (result: Result<string, string>) =
        match result with
        | Ok reservation -> printfn $"Reservation confirmed:\n{reservation}"
        | Error ex -> printfn $"Reservation failed:\n {ex}"

    let postReservation url request =
        use client = ApiClient.client
        ApiClient.postAsync client url request

    let exit code = code

    let die (ex: Exception) =
        printfn "Exiting caught: %s" ex.Message
        exit -1

    [<EntryPoint>]
    let main (argv: string array) =
        try
            parser.Parse(argv)
            |> buildRequest
            ||> postReservation
            |> parseResults

            exit 0
        with
        | :? ArguParseException as ex ->
            usage parser
            die ex
        | ex -> die ex
