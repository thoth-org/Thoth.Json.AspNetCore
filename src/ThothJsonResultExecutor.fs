namespace Thoth.Json.AspNetCore

open System
open System.IO
open System.Threading.Tasks
open System.Text

open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Mvc.Infrastructure
open Microsoft.Extensions.Primitives
open Microsoft.Net.Http.Headers

open FSharp.Control.Tasks.V2
open Newtonsoft.Json
open Thoth.Json.Net

[<AllowNullLiteral>]
type ThothJsonResultExecutor(writerFactory: IHttpResponseStreamWriterFactory
                            ,options: ThothJsonOptions) =
    do
        if isNull writerFactory then raise (ArgumentNullException "writerFactory")
        if isNull options then raise (ArgumentNullException "options")

    let defaultContentType =
        let contentType = MediaTypeHeaderValue(StringSegment "application/json")
        contentType.Encoding <- Encoding.UTF8
        contentType.ToString()

    interface IActionResultExecutor<JsonResult> with
        member _.ExecuteAsync(context, result) =
            if isNull context then raise (ArgumentNullException "context")
            if isNull result then raise (ArgumentNullException "result")

            let response = context.HttpContext.Response
            response.ContentType <- defaultContentType

            if result.StatusCode.HasValue then
                response.StatusCode <- result.StatusCode.Value

            use streamWriter = writerFactory.CreateWriter(response.Body, UTF8Encoding())
            use jsonWriter = new JsonTextWriter(streamWriter)

            let t = result.Value.GetType()
            let encoder = Encode.Auto.LowLevel.generateEncoderCached (t, options.IsCamelCase)

            upcast task {
                       do! (encoder result.Value).WriteToAsync(jsonWriter)
                       do! streamWriter.FlushAsync()
                   }
