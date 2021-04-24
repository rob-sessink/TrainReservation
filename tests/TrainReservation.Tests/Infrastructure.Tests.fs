namespace TrainReservation.Tests

open Xunit.Abstractions

// Based on: https://github.com/jet/equinox/blob/master/samples/Store/Integration/Infrastructure.fs
module Infrastructure =

    // Derived from https://github.com/damianh/CapturingLogOutputWithXunit2AndParallelTests
    type TestOutputAdapter(testOutput: ITestOutputHelper) =
        let formatter =
            Serilog.Formatting.Display.MessageTemplateTextFormatter(
                "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] {Message}{NewLine}{Exception}",
                null
            )

        let writeSerilogEvent logEvent =
            use writer = new System.IO.StringWriter()
            formatter.Format(logEvent, writer)
            writer |> string |> testOutput.WriteLine

        interface Serilog.Core.ILogEventSink with
            member _.Emit logEvent = writeSerilogEvent logEvent

[<AutoOpen>]
module SerilogHelpers =
    open Serilog

    let createLogger sink =
        LoggerConfiguration()
            .WriteTo.Sink(sink)
            //.WriteTo.Seq("http://localhost:5341")
            .CreateLogger()
