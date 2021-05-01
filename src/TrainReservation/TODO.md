# TicketOffice.Controller

# Event Sourcing

- Add a few decoder / encoder tests
- Add directory structure around package and event sourcing module

## Percentage UoM

Try out if making *percentage* a unit of measure add benefits
https://www.devjoy.com/blog/reading-fsharp/
https://stackoverflow.com/questions/31439855/using-math-round-with-a-unit-of-measure
https://stackoverflow.com/questions/3791959/f-units-of-measure-conversion-based-on-type?rq=1

# ErrorHandling in Flows

In ReserveSeatsFlow use FsToolkit.ErrorHandling Trail computational expression for more flexibility in validation and
flow

# Build Process

Fix running of fsharp-analyzers in build.fsx

// ==> "FSharpAnalyzers" disable because of Unhandled exception. System.InvalidOperationException: The input list was
empty.
