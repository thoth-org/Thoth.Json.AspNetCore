namespace Microsoft.Extensions.DependencyInjection

open System
open System.Runtime.CompilerServices
open Microsoft.AspNetCore.Mvc.Infrastructure
open Microsoft.AspNetCore.Mvc
open Thoth.Json.AspNetCore

[<Extension>]
type ThothJsonMvcCoreBuilderExtensions () =
    [<Extension>]
    static member AddThothJson (builder: IMvcCoreBuilder, setupOptions: Func<ThothJsonOptions>) =
        if isNull builder then raise (ArgumentNullException("builder"))
        let options = setupOptions.Invoke()
        builder.Services.AddSingleton<ThothJsonOptions>(options) |> ignore
        builder.Services.AddSingleton<IActionResultExecutor<JsonResult>, ThothJsonResultExecutor>()
