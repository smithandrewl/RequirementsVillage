namespace RequirementsVillage.Api.Tests.Integration

open System
open Xunit
open FsUnit.Xunit
open RequirementsVillage.Api.Tests.Helpers

module HealthEndpointTests =
  
  [<Fact>]
  let ``GET /api/health should return 200 OK`` () =
    async {
      use factory = new TestWebApplicationFactory()
      use client = ApiTestHelpers.createClient factory
      
      let! response = client |> ApiTestHelpers.get "/api/health"
      
      ApiTestHelpers.shouldBeOk response
    } |> TestHelpers.runAsync
  
  [<Fact>]
  let ``GET /api/health should return health status with timestamp`` () =
    async {
      use factory = new TestWebApplicationFactory()
      use client = ApiTestHelpers.createClient factory
      
      let beforeRequest = DateTime.UtcNow
      let! response = client |> ApiTestHelpers.get "/api/health"
      let afterRequest = DateTime.UtcNow
      
      ApiTestHelpers.shouldBeOk response
      
      let content = ApiTestHelpers.getResponseContent response
      
      content |> should haveSubstring "\"status\":\"healthy\""
      content |> should haveSubstring "\"timestamp\""
      
      // Parse the response
      let! health = ApiTestHelpers.getResponseJson<{| Status: string; Timestamp: DateTime |}> response
      
      health.Status |> should equal "healthy"
      health.Timestamp |> should be (greaterThanOrEqualTo beforeRequest)
      health.Timestamp |> should be (lessThanOrEqualTo afterRequest)
    } |> TestHelpers.runAsync
  
  [<Fact>]
  let ``Health endpoint should be accessible without authentication`` () =
    async {
      use factory = new TestWebApplicationFactory()
      use client = ApiTestHelpers.createClient factory
      
      // Clear any default headers
      client.DefaultRequestHeaders.Clear()
      
      let! response = client |> ApiTestHelpers.get "/api/health"
      
      ApiTestHelpers.shouldBeOk response
    } |> TestHelpers.runAsync
  
  [<Fact>]
  let ``Health endpoint should support HEAD requests`` () =
    async {
      use factory = new TestWebApplicationFactory()
      use client = ApiTestHelpers.createClient factory
      
      use request = new System.Net.Http.HttpRequestMessage(
        System.Net.Http.HttpMethod.Head,
        "/api/health"
      )
      
      let! response = client.SendAsync(request) |> Async.AwaitTask
      
      ApiTestHelpers.shouldBeOk response
      let content = ApiTestHelpers.getResponseContent response
      content |> should equal "" // HEAD requests should have no body
    } |> TestHelpers.runAsync