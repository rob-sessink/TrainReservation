namespace TrainReservation.TicketOffice

module WebApp =

    open System
    open Microsoft.AspNetCore.Builder
    open Microsoft.AspNetCore.Hosting
    open Microsoft.Extensions.Hosting
    open Microsoft.Extensions.DependencyInjection
    open Microsoft.Extensions.Logging
    open Giraffe
    open TrainReservation

    let webApp =
        choose [ GET
                 >=> choose [ route "/" >=> text "Hello Traveller!" ]
                 POST
                 >=> choose [ route "/reserve"
                              >=> Root.ComposeReservationHandler
                              routef "/reset/%s" Root.ComposeResetTrainHandler ]
                 setStatusCode 404 >=> text "Not Found" ]

    let errorHandler (ex: Exception) (logger: ILogger) =
        logger.LogError(EventId(), ex, "An unhandled exception has occurred while executing the request.")

        clearResponse
        >=> setStatusCode 500
        >=> text ex.Message

    let configureApp (app: IApplicationBuilder) =
        app.UseGiraffeErrorHandler errorHandler |> ignore
        app.UseGiraffe webApp

    let configureLogging (builder: ILoggingBuilder) =
        // Set a logging filter (optional)
        let filter (l: LogLevel) = l.Equals LogLevel.Information

        builder.AddFilter(filter).AddConsole().AddDebug()
        |> ignore

    let configureServices (services: IServiceCollection) = services.AddGiraffe() |> ignore

    let server () =
        Host
            .CreateDefaultBuilder()
            .ConfigureWebHostDefaults(fun webHost ->
                webHost
                    .Configure(configureApp)
                    .ConfigureServices(configureServices)
                |> ignore)
            .Build()
            .Run()

        0
