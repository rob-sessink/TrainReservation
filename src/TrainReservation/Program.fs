module TrainReservation.Program

open TrainReservation.TicketOffice.WebApp

open System.Reflection

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

    let printVersion() =
        let version = assembly.Force().GetName().Version
        printfn "%A" version

    let printInfo() =
        let assembly = assembly.Force()
        let name = assembly.GetName()
        let version = assembly.GetName().Version
        let releaseDate = getReleaseDate assembly
        let gitHash = getGitHash assembly
        printfn "%s - %A - %s - %s" name.Name version releaseDate gitHash


module Main =
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

    [<EntryPoint>]
    let main (argv: string array) =
        let parser = ArgumentParser.Create<CLIArguments>(programName = "TrainReservation")

        let results = parser.Parse(argv)

        if results.Contains Version then AssemblyInfo.printVersion()
        elif results.Contains Info then AssemblyInfo.printInfo()
        elif results.Contains Run then server |> ignore
        else parser.PrintUsage() |> printfn "%s"

        0
