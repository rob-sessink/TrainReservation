namespace TrainReservation

/// <summary>TimeProvider provided a mechanism to influence notion of 'Time' to the application. Allowing the
/// usage of a standard system clock, a fixed clock (i.e. unit testing), historical/future time or a
/// sequence of time values.
///
/// let systemTime = new TimeProvider.SystemTime()
/// let fixedCurrent = TimeProvider.CurrentFixed()
/// let fixedTime = TimeProvider.By(FixedTimeProvider.From(DateTime(2021, 01, 01, 12, 00, 00)))
///
/// https://blog.submain.com/4-common-datetime-mistakes-c-avoid/
/// </summary>
module TimeProvider =

    open System

    // interface defining notions of
    type ITimeProvider =
        abstract Now: DateTimeOffset

    // <summary>System time in UTC</summary>
    type SystemTimeProvider() =
        interface ITimeProvider with
            member this.Now = DateTimeOffset.UtcNow

    // <summary>Fixed time</summary>
    type FixedTimeProvider(fdt: DateTimeOffset) =
        let time = fdt

        interface ITimeProvider with
            member this.Now = time

        // <summary>Fixed time initialized on current UTC time</summary>
        static member Now() = FixedTimeProvider(DateTimeOffset.UtcNow)

        // <summary>Fixed time initialized via a DateTimeOffset</summary>
        static member From(fdt: DateTimeOffset) = FixedTimeProvider(fdt)

    // <summary>TimeProvider</summary>
    // <param name="provider">strategy</summary>
    type TimeProvider private (provider: ITimeProvider) =
        let Provider = provider
        member this.Now = Provider.Now

        static member SystemTime() = TimeProvider(SystemTimeProvider())
        static member CurrentFixed() = TimeProvider(FixedTimeProvider.Now())
        static member By(provider) = TimeProvider(provider)

/// <summary>Time provider for the application, defaulting to the system time)</summary>
module ApplicationTime =

    let mutable time = TimeProvider.TimeProvider.SystemTime()
