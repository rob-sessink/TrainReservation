# TicketOffice.Controller

# Event Sourcing

- Add a few decoder / encoder tests
- Add directory structure around package and event sourcing module

# Constrained Types

Model TrainId and SeatId etc. as constrained types preferably with a Result and DomainMessage

With this we also need to adjust the Thoth decoders:
a) properly work with these constrained types as we map directly from JSON towards the type b) use DTO types with
primitives to which Thoth decodes and map later to validated Domain types

For a) we probably need to use a Decoder function per constrain type and map an Error to DecoderError? however this
feels the cleanest solution

https://fsharpforfunandprofit.com/posts/designing-with-types-single-case-dus/
https://github.com/swlaschin/Railway-Oriented-Programming-Example/blob/master/src/FsRopExample/DomainModel.fs
https://thoth-org.github.io/Thoth.Json/#Decoding-Objects

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
