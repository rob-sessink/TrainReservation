module TrainReservation.Program

open System
open System.Reflection
open TrainReservation.TicketOffice

module AssemblyInfo =

    let metaDataValue (mda: AssemblyMetadataAttribute) = mda.Value

    let getMetaDataAttribute (assembly: Assembly) key =
        assembly.GetCustomAttributes(typedefof<AssemblyMetadataAttribute>)
        |> Seq.cast<AssemblyMetadataAttribute>
        |> Seq.find (fun x -> x.Key = key)

    let getReleaseDate assembly =
        "ReleaseDate"
        |> getMetaDataAttribute assembly
        |> metaDataValue

    let getGitHash assembly =
        "GitHash"
        |> getMetaDataAttribute assembly
        |> metaDataValue

    let getVersion assembly =
        "AssemblyVersion"
        |> getMetaDataAttribute assembly
        |> metaDataValue

    let assembly = lazy (Assembly.GetEntryAssembly())

    let printVersion () =
        let version = assembly.Force().GetName().Version
        printfn $"%A{version}"

    let printInfo () =
        let assembly = assembly.Force()
        let name = assembly.GetName()
        let version = assembly.GetName().Version
        let releaseDate = getReleaseDate assembly
        let gitHash = getGitHash assembly
        printfn $"%s{name.Name} - %A{version} - %s{releaseDate} - %s{gitHash}"


module Arguments =

    open Argu

    type CLIArguments =
        | Info
        | Version
        | Run
        interface IArgParserTemplate with
            member s.Usage =
                match s with
                | Info -> "More detailed information"
                | Version -> "Version of application"
                | Run -> "Run TicketOffice API"

    let errorHandler =
        ProcessExiter
            (colorizer =
                function
                | ErrorCode.HelpText -> None
                | _ -> Some ConsoleColor.Red)

    let usage (parser: ArgumentParser) = printfn "%s" <| parser.PrintUsage()

    let parser =
        ArgumentParser.Create<CLIArguments>(programName = "TrainReservation", errorHandler = errorHandler)

module Main =

    open Argu
    open Arguments

    let exit code = code

    let die (ex: Exception) =
        printfn $"Exiting caught: %s{ex.Message}"
        exit -1

    [<EntryPoint>]
    let main (argv: string array) =

        try
            let results = parser.Parse(argv)

            if results.Contains Version then AssemblyInfo.printVersion ()
            elif results.Contains Info then AssemblyInfo.printInfo ()
            elif results.Contains Run then WebApp.server () |> ignore
            else usage parser

            exit 0
        with
        | :? ArguParseException as ex ->
            usage parser
            die ex
        | ex -> die ex
