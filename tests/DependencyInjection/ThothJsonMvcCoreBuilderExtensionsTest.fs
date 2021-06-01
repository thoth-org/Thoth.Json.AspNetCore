module ThothJsonMvcCoreBuilderExtensionsTest

open Expecto
open Moq
open Microsoft.Extensions.DependencyInjection


[<Tests>]
let tests =

  testList "ThothJsonMvcCoreBuilderExtensions" [

    testCase "ensure AddThothJson is present in IMvcCoreBuilder, configures and returns builder back" <| fun _ ->
      // Arrange
      let serviceCollection = ServiceCollection()
      let builder = Mock<IMvcCoreBuilder>()
      builder
        .Setup(fun b -> b.Services)
        .Returns(serviceCollection) |> ignore

      // Act
      let returnedBuilder = builder.Object.AddThothJson(fun _ -> ())

      // Assert
      Expect.equal returnedBuilder builder.Object "builder returned?"
  ]