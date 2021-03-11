namespace TrainReservation.Tests

module HttpContextUtil =

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

    /// <summary>Mocked next HttpFun</summary>
    let next: HttpFunc = Some >> Task.FromResult

    /// <summary>Build an HTTPContext mock</summary>
    let buildMockContext () =
        let ctx = Substitute.For<HttpContext>()

        ctx
            .RequestServices
            .GetService(typeof<INegotiationConfig>)
            .Returns(DefaultNegotiationConfig())
        |> ignore

        ctx
            .RequestServices
            .GetService(typeof<Json.IJsonSerializer>)
            .Returns(NewtonsoftJsonSerializer(NewtonsoftJsonSerializer.DefaultSettings))
        |> ignore

        ctx.Request.Headers.ReturnsForAnyArgs(new HeaderDictionary())
        |> ignore

        ctx.Response.Body <- new MemoryStream()
        ctx

    /// <summary>Read the body from the response</summary>
    /// <param name="ctx">of the request</param>
    let getBody (ctx: HttpContext) =
        ctx.Response.Body.Position <- 0L

        use reader =
            new StreamReader(ctx.Response.Body, System.Text.Encoding.UTF8)

        reader.ReadToEnd()

    /// <summary>Set the body context on the request</summary>
    /// <param name="ctx">of the request</param>
    /// <param name="body">to set on the request</param>
    let setBody (ctx: HttpContext) (body: 'a) =
        let data =
            Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(body))

        ctx.Request.Body <- new MemoryStream(data)
        ctx

    /// <summary>Build the mock context for a handler</summary>
    /// <param name="method">executed (HTTP verb)</param>
    /// <param name="path">called</param>
    /// <param name="body">of the request as (object/record)</param>
    let buildHandlerContext (method: string) (path: string) (body: 'a option) =
        let ctx = buildMockContext ()

        Option.map (setBody ctx) body |> ignore

        ctx.Request.Method.ReturnsForAnyArgs method
        |> ignore

        ctx.Request.Path.ReturnsForAnyArgs(PathString(path))
        |> ignore

        ctx
