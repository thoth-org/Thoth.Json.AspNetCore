namespace Thoth.Json.AspNetCore

[<AllowNullLiteral>]
type ThothJsonOptions () =
    member val IsCamelCase = false with get, set
