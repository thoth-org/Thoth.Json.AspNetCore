module ThothJsonMvcBuilderExtensionsTest

open Expecto
open Moq
open Microsoft.Extensions.DependencyInjection


[<Tests>]
let tests =

  testList "ThothJsonMvcBuilderExtensions" [

    testCase "ensure AddThothJson is present in IMvcBuilder, configures and returns builder back" <| fun _ ->
      // Arrange
      let serviceCollection = ServiceCollection()
      let builder = Mock<IMvcBuilder>()
      builder
        .Setup(fun b -> b.Services)
        .Returns(serviceCollection) |> ignore

      // Act
      let returnedBuilder = builder.Object.AddThothJson(fun _ -> ())

      // Assert
      Expect.equal returnedBuilder builder.Object "builder returned?"
  ]