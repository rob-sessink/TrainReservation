module TrainReservation.Tests.HttpContextUtil

open System.IO
open System.Text
open System.Threading.Tasks
open Giraffe
open Giraffe.Serialization
open Microsoft.AspNetCore.Http
open NSubstitute

let next: HttpFunc = Some >> Task.FromResult

let buildMockContext () =
    let context = Substitute.For<HttpContext>()

    context
        .RequestServices
        .GetService(typeof<INegotiationConfig>)
        .Returns(DefaultNegotiationConfig())
    |> ignore

    context
        .RequestServices
        .GetService(typeof<Json.IJsonSerializer>)
        .Returns(NewtonsoftJsonSerializer(NewtonsoftJsonSerializer.DefaultSettings))
    |> ignore

    context.Request.Headers.ReturnsForAnyArgs(new HeaderDictionary())
    |> ignore

    context.Response.Body <- new MemoryStream()
    context

let getBody (ctx: HttpContext) =
    ctx.Response.Body.Position <- 0L

    use reader =
        new StreamReader(ctx.Response.Body, System.Text.Encoding.UTF8)

    reader.ReadToEnd()
