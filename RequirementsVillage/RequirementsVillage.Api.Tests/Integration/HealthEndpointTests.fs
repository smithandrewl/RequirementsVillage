namespace RequirementsVillage.Api.Tests.Integration

open Xunit
open FsUnit.Xunit
open RequirementsVillage.Api.Tests.Helpers

/// Integration tests for the Health Check endpoint
module HealthEndpointTests =
  
  [<Fact>]
  let ``GET /api/health should return 200 OK`` () =
    async {
      use factory = new TestWebApplicationFactory()
      use client = factory.CreateClient()
      
      let! response = client |> ApiTestHelpers.get "/api/health"
      
      ApiTestHelpers.shouldBeOk response
    } |> TestHelpers.runAsync