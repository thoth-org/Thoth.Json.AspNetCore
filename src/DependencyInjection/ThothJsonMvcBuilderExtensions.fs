namespace Microsoft.Extensions.DependencyInjection

open System
open System.Runtime.CompilerServices
open Microsoft.AspNetCore.Mvc.Infrastructure
open Microsoft.AspNetCore.Mvc
open Thoth.Json.AspNetCore

[<Extension>]
type ThothJsonMvcBuilderExtensions () =
    [<Extension>]
    static member AddThothJson (builder: IMvcBuilder, setupOptions: Action<ThothJsonOptions>) =
        if isNull builder then raise (ArgumentNullException("builder"))
        if isNull setupOptions then raise (ArgumentNullException("setupOptions"))
        let options = ThothJsonOptions()
        setupOptions.Invoke(options)
        builder.Services.AddSingleton<ThothJsonOptions>(options) |> ignore
        builder.Services.AddSingleton<IActionResultExecutor<JsonResult>, ThothJsonResultExecutor>() |> ignore
        builder
