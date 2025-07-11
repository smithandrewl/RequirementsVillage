namespace RequirementsVillage.Api.Tests.Integration

open System
open Xunit
open FsUnit.Xunit
open RequirementsVillage.Shared
open RequirementsVillage.Api.Tests.Helpers

/// Integration tests for Project API endpoints
module ProjectEndpointsTests =
  
  module CreateProject =
    
    [<Fact>]
    let ``POST /api/projects should return 201 Created with new project`` () =
      async {
        use factory = new TestWebApplicationFactory()
        use client = ApiTestHelpers.createClientWithInMemoryData factory
        
        let request = ApiTestHelpers.createProjectRequest
                        "New Test Project"
                        "This is a test project"
                        "webApp"
        
        let content = ApiTestHelpers.createJsonContent request
        let! response = client |> ApiTestHelpers.post "/api/projects" content
        
        ApiTestHelpers.shouldBeCreated response
        
        let content = ApiTestHelpers.getResponseContent response
        let project = content |> ApiTestHelpers.fromJson<Project>
        project.Name |> should equal "New Test Project"
        project.Description |> should equal "This is a test project"
        project.Category |> should equal WebApp
        project.Status |> should equal Idea
        
        // Verify Location header
        response.Headers.Location |> should not' (be null)
        response.Headers.Location.ToString() |> should haveSubstring $"/api/projects/{project.Id}"
      } |> TestHelpers.runAsync