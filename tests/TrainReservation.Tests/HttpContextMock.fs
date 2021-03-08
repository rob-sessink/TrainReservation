module TrainReservation.Tests.HttpContextUtil

open System.IO
open System.Text
open System.Threading.Tasks
open Giraffe
open Giraffe.Serialization
open Microsoft.AspNetCore.Http
open NSubstitute
open Newtonsoft.Json

/// ---------------------------------------------------------------------------
/// Functions to mock HttpContext for testing of HttpHandler

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

let buildHandlerContext (method: string) (path: string) request =
    let postData =
        Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(request))

    let context = buildMockContext ()
    context.Request.Body <- new MemoryStream(postData)

    context.Request.Method.ReturnsForAnyArgs method
    |> ignore

    context.Request.Path.ReturnsForAnyArgs(PathString(path))
    |> ignore

    context.Request.Body <- new MemoryStream(postData)
    context
