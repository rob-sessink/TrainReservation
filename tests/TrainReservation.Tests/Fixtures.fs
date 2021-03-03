module TrainReservation.Tests.Fixtures

open System.IO

/// <summary>Return contents of a fixture as string. Ending newline is stripped of</summary>
/// <param name="relativePath">relative path to fixture from project root</param>
let readFixture relativePath =
    let content = File.ReadAllText(relativePath)

    if content.EndsWith "\n" then content.TrimEnd('\n') else content
