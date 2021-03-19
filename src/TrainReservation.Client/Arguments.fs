namespace TrainReservation.Client

module Arguments =

    open Argu
    open System
    open TrainReservation.Client.ApiTypes

    type CLIArguments =
        | [<Mandatory>] URL of url: string
        | [<Mandatory>] TrainId of trainId: string
        | [<Mandatory>] Seats of seats: int

        interface IArgParserTemplate with
            member s.Usage =
                match s with
                | URL _ -> "URL of the TrainReservation API"
                | TrainId _ -> "Train identifier to reserve as seats for"
                | Seats _ -> "Number of seats to reserve"

    let printArguments (arguments: Argu.ParseResults<CLIArguments>) =
        printfn "Calling %A" <| arguments
        arguments

    let buildRequest (arguments: Argu.ParseResults<CLIArguments>) =
        let url = arguments.GetResult URL
        let trainId = arguments.GetResult TrainId
        let seats = arguments.GetResult Seats

        (url, ({ trainId = trainId; seats = seats }))

    let errorHandler =
        ProcessExiter
            (colorizer =
                function
                | ErrorCode.HelpText -> None
                | _ -> Some ConsoleColor.Red)

    let usage (parser: ArgumentParser) = printfn "%s" <| parser.PrintUsage()

    let parser =
        ArgumentParser.Create<CLIArguments>(programName = "TrainReservation.Client", errorHandler = errorHandler)
