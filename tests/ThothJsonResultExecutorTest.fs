module Tests

open System
open System.Collections.Generic
open System.IO
open System.Text

open Microsoft.AspNetCore.Mvc
open Microsoft.AspNetCore.Mvc.Infrastructure
open Microsoft.AspNetCore.Http

open Thoth.Json.Net
open Thoth.Json.AspNetCore
open Expecto
open Moq


[<Tests>]
let tests =

  let intendationLength = 0

  testList "ThothJsonResultExecutor" [

    testCase "should create ThothJsonResultExecutor instance" <| fun _ ->
      // Arrange
      let writerFactory = Mock<IHttpResponseStreamWriterFactory>()
      // Act
      let instance = ThothJsonResultExecutor(writerFactory.Object, ThothJsonOptions())
      // Assert
      Expect.isNotNull instance "isNotNull?"


    testCase "should throw ArgumentNullException when IHttpResponseStreamWriterFactory is not provided" <| fun _ ->
      // Assert
      Expect.throws (fun _ -> ThothJsonResultExecutor(null, ThothJsonOptions()) |> ignore) "throws exception?"


    testCase "should throw ArgumentNullException when ThothJsonOptions is not provided" <| fun _ ->
      // Arrange
      let writerFactory = Mock<IHttpResponseStreamWriterFactory>()
      // Assert
      Expect.throws (fun _ -> ThothJsonResultExecutor(writerFactory.Object, null) |> ignore) "throws exception?"

    testCase "ExecuteAsync should throw ArgumentNullException when ActionContext is not provided" <| fun _ ->
       // Arrange
       let writerFactory = Mock<IHttpResponseStreamWriterFactory>()
       // Act
       let instance = ThothJsonResultExecutor(writerFactory.Object, ThothJsonOptions())
                      :> IActionResultExecutor<JsonResult>
       // Assert
       Expect.throws (fun _ -> instance.ExecuteAsync(null, JsonResult(Some "Result")) |> ignore) "throws exception?"


    testCase "ExecuteAsync should throw ArgumentNullException when JsonResult is not provided" <| fun _ ->
       // Arrange
       let writerFactory = Mock<IHttpResponseStreamWriterFactory>()
       // Act
       let instance = ThothJsonResultExecutor(writerFactory.Object, ThothJsonOptions())
                      :> IActionResultExecutor<JsonResult>
       // Assert
       Expect.throws (fun _ -> instance.ExecuteAsync(ActionContext(), null) |> ignore) "throws exception?"

    
    testTask "should write StatusCode to Response if status code is set in ActionResult" {
      // Arrange
      let stream = new MemoryStream()
      let textWriter = new StreamWriter(stream)
      let writerFactory = Mock<IHttpResponseStreamWriterFactory>()
      writerFactory
        .Setup(fun w -> w.CreateWriter(stream, It.IsAny<Encoding>()))
        .Returns(textWriter) |> ignore
      
      let mutable actualStatusCode = 0
      let expectedStatusCode = 201

      let httpContext = Mock<HttpContext>()
      httpContext.Setup(fun ctx -> ctx.Response.Body).Returns(stream) |> ignore
      httpContext
        .SetupSet(fun ctx -> ctx.Response.StatusCode)
        .Callback(fun value -> actualStatusCode <- value) |> ignore

      let actionContext = ActionContext (HttpContext = httpContext.Object)

      let value = Some (Ok "Result")
      let result = JsonResult(value)
      result.StatusCode <- Nullable(expectedStatusCode)

      // Act
      let instance = ThothJsonResultExecutor(writerFactory.Object, ThothJsonOptions())
                     :> IActionResultExecutor<JsonResult>
      do! instance.ExecuteAsync(actionContext, result)

      // Assert
      Expect.equal actualStatusCode expectedStatusCode "StatusCode equal?"
    }


    testTask "should serialize a F# type as expected" {
      // Arrange
      let stream = new MemoryStream()
      let textWriter = new StreamWriter(stream)
      let writerFactory = Mock<IHttpResponseStreamWriterFactory>()
      writerFactory
        .Setup(fun w -> w.CreateWriter(stream, It.IsAny<Encoding>()))
        .Returns(textWriter) |> ignore
      
      let httpContext = Mock<HttpContext>()
      httpContext.Setup(fun ctx -> ctx.Response.Body).Returns(stream) |> ignore
      let actionContext = ActionContext (HttpContext = httpContext.Object)

      let value = Some (Ok "Result")
      let result = JsonResult(value)
      let expectedSerializedString = Encode.Auto.toString(intendationLength, value)

      // Act
      let instance =
        ThothJsonResultExecutor(writerFactory.Object, ThothJsonOptions())
        :> IActionResultExecutor<JsonResult>
      do! instance.ExecuteAsync(actionContext, result)

      // Assert
      let actualSerializedString = System.Text.Encoding.Default.GetString(stream.ToArray());
      Expect.equal actualSerializedString expectedSerializedString "Json equal?"
    }

    testTask "should serialize a F# type as expected with CamelCase setting `True`" {
      // Arrange
      let stream = new MemoryStream()
      let textWriter = new StreamWriter(stream)
      let writerFactory = Mock<IHttpResponseStreamWriterFactory>()
      writerFactory
        .Setup(fun w -> w.CreateWriter(stream, It.IsAny<Encoding>()))
        .Returns(textWriter) |> ignore
      
      let httpContext = Mock<HttpContext>()
      httpContext.Setup(fun ctx -> ctx.Response.Body).Returns(stream) |> ignore
      let actionContext = ActionContext (HttpContext = httpContext.Object)

      let isCamelCase = true
      let value = {| Id = 1; Name = "FooBar" |}
      let result = JsonResult(value)
      let expectedSerializedString = Encode.Auto.toString(intendationLength, value, isCamelCase)

      // Act
      let instance =
          ThothJsonResultExecutor(writerFactory.Object, ThothJsonOptions(IsCamelCase = isCamelCase))
          :> IActionResultExecutor<JsonResult>

      do! instance.ExecuteAsync(actionContext, result)

      // Assert
      let actualSerializedString = System.Text.Encoding.Default.GetString(stream.ToArray());
      Expect.equal actualSerializedString expectedSerializedString "Json equal?"
    }


    testTask "should serialize a CLR/C# type as expected" {
      // Arrange
      let stream = new MemoryStream()
      let textWriter = new StreamWriter(stream)
      let writerFactory = Mock<IHttpResponseStreamWriterFactory>()
      writerFactory
        .Setup(fun w -> w.CreateWriter(stream, It.IsAny<Encoding>()))
        .Returns(textWriter) |> ignore
      
      let httpContext = Mock<HttpContext>()
      httpContext.Setup(fun ctx -> ctx.Response.Body).Returns(stream) |> ignore
      let actionContext = ActionContext (HttpContext = httpContext.Object)

      let value = KeyValuePair("Foo", "Bar")
      let result = JsonResult(value)
      let expectedSerializedString = Encode.Auto.toString(intendationLength, value)

      // Act
      let instance = ThothJsonResultExecutor(writerFactory.Object, ThothJsonOptions()) :> IActionResultExecutor<JsonResult>
      do! instance.ExecuteAsync(actionContext, result)

      // Assert
      let actualSerializedString = System.Text.Encoding.Default.GetString(stream.ToArray());
      Expect.equal actualSerializedString expectedSerializedString "Json equal?"
    }
  ]
