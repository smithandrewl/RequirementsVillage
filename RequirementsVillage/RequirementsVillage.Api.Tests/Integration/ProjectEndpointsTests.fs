namespace RequirementsVillage.Api.Tests.Integration

open System
open Xunit
open FsUnit.Xunit
open RequirementsVillage.Shared
open RequirementsVillage.Api.Tests.Helpers
open RequirementsVillage.Shared.TestGenerators

module ProjectEndpointsTests =
  
  module GetAllProjects =
    
    [<Fact>]
    let ``GET /api/projects should return 200 OK with project list`` () =
      async {
        use factory = new TestWebApplicationFactory()
        use client = ApiTestHelpers.createClientWithInMemoryData factory
        
        let! response = client |> ApiTestHelpers.get "/api/projects"
        
        ApiTestHelpers.shouldBeOk response
        
        let! projects = ApiTestHelpers.getResponseJson<Project list> response
        projects |> should not' (be Empty)
      } |> TestHelpers.runAsync
    
    [<Fact>]
    let ``GET /api/projects should return empty array when no projects exist`` () =
      async {
        use factory = new TestWebApplicationFactory()
        let emptyRepo = Fixtures.Mocks.ConfigurableMockRepository()
        emptyRepo.SetProjects([])
        use client = ApiTestHelpers.createClientWithMockRepo emptyRepo factory
        
        let! response = client |> ApiTestHelpers.get "/api/projects"
        
        ApiTestHelpers.shouldBeOk response
        
        let! projects = ApiTestHelpers.getResponseJson<Project list> response
        projects |> should be Empty
      } |> TestHelpers.runAsync
  
  module GetProjectById =
    
    [<Fact>]
    let ``GET /api/projects/{id} should return 200 OK for existing project`` () =
      async {
        use factory = new TestWebApplicationFactory()
        let testProject = TestDataGenerators.Bogus.Default.project()
        let mockRepo = Fixtures.Mocks.ConfigurableMockRepository()
        mockRepo.SetProjects([testProject])
        use client = ApiTestHelpers.createClientWithMockRepo mockRepo factory
        
        let! response = client |> ApiTestHelpers.get $"/api/projects/{testProject.Id}"
        
        ApiTestHelpers.shouldBeOk response
        
        let content = ApiTestHelpers.getResponseContent response
        let project = content |> ApiTestHelpers.fromJson<Project>
        project.Id |> should equal testProject.Id
        project.Name |> should equal testProject.Name
      } |> TestHelpers.runAsync
    
    [<Fact>]
    let ``GET /api/projects/{id} should return 404 NotFound for non-existent project`` () =
      async {
        use factory = new TestWebApplicationFactory()
        use client = ApiTestHelpers.createClientWithInMemoryData factory
        
        let nonExistentId = Guid.NewGuid()
        let! response = client |> ApiTestHelpers.get $"/api/projects/{nonExistentId}"
        
        ApiTestHelpers.shouldBeNotFound response
      } |> TestHelpers.runAsync
    
    [<Fact>]
    let ``GET /api/projects/{id} should return 400 BadRequest for invalid GUID`` () =
      async {
        use factory = new TestWebApplicationFactory()
        use client = ApiTestHelpers.createClientWithInMemoryData factory
        
        let! response = client |> ApiTestHelpers.get "/api/projects/invalid-guid"
        
        ApiTestHelpers.shouldBeBadRequest response
      } |> TestHelpers.runAsync
  
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
    
    [<Fact>]
    let ``POST /api/projects should return 400 BadRequest for empty name`` () =
      async {
        use factory = new TestWebApplicationFactory()
        use client = ApiTestHelpers.createClientWithInMemoryData factory
        
        let request = ApiTestHelpers.createProjectRequest "" "Description" "tool"
        
        let content = ApiTestHelpers.createJsonContent request
        let! response = client |> ApiTestHelpers.post "/api/projects" content
        
        ApiTestHelpers.shouldBeBadRequest response
        
        let errorContent = ApiTestHelpers.getResponseContent response
        errorContent |> should haveSubstring "name"
        errorContent |> should haveSubstring "empty"
      } |> TestHelpers.runAsync
    
    [<Fact>]
    let ``POST /api/projects should return 400 BadRequest for invalid category`` () =
      async {
        use factory = new TestWebApplicationFactory()
        use client = ApiTestHelpers.createClientWithInMemoryData factory
        
        let jsonContent = """{"name":"Test","description":"Test","category":"invalid"}"""
        let content = new System.Net.Http.StringContent(
          jsonContent,
          System.Text.Encoding.UTF8,
          "application/json"
        )
        
        let! response = client |> ApiTestHelpers.post "/api/projects" content
        
        ApiTestHelpers.shouldBeBadRequest response
      } |> TestHelpers.runAsync
  
  module UpdateProject =
    
    [<Fact>]
    let ``PUT /api/projects/{id} should return 204 NoContent for successful update`` () =
      async {
        use factory = new TestWebApplicationFactory()
        let existingProject = TestDataGenerators.Bogus.Default.project()
        let mockRepo = Fixtures.Mocks.ConfigurableMockRepository()
        mockRepo.SetProjects([existingProject])
        use client = ApiTestHelpers.createClientWithMockRepo mockRepo factory
        
        let updatedProject = {
          existingProject with
            Name = "Updated Name"
            Description = "Updated Description"
        }
        
        let content = ApiTestHelpers.createJsonContent updatedProject
        let! response = client |> ApiTestHelpers.put $"/api/projects/{existingProject.Id}" content
        
        ApiTestHelpers.shouldBeNoContent response
        
        // Verify the update
        let! getResponse = client |> ApiTestHelpers.get $"/api/projects/{existingProject.Id}"
        let getContent = ApiTestHelpers.getResponseContent getResponse
        let project = getContent |> ApiTestHelpers.fromJson<Project>
        
        project.Name |> should equal "Updated Name"
        project.Description |> should equal "Updated Description"
      } |> TestHelpers.runAsync
    
    [<Fact>]
    let ``PUT /api/projects/{id} should return 404 NotFound for non-existent project`` () =
      async {
        use factory = new TestWebApplicationFactory()
        use client = ApiTestHelpers.createClientWithInMemoryData factory
        
        let nonExistentProject = TestDataGenerators.Bogus.Default.project()
        let content = ApiTestHelpers.createJsonContent nonExistentProject
        let! response = client |> ApiTestHelpers.put $"/api/projects/{nonExistentProject.Id}" content
        
        ApiTestHelpers.shouldBeNotFound response
      } |> TestHelpers.runAsync
    
    [<Fact>]
    let ``PUT /api/projects/{id} should return 400 BadRequest for ID mismatch`` () =
      async {
        use factory = new TestWebApplicationFactory()
        use client = ApiTestHelpers.createClientWithInMemoryData factory
        
        let project = TestDataGenerators.Bogus.Default.project()
        let differentId = Guid.NewGuid()
        
        let content = ApiTestHelpers.createJsonContent project
        let! response = client |> ApiTestHelpers.put $"/api/projects/{differentId}" content
        
        ApiTestHelpers.shouldBeBadRequest response
      } |> TestHelpers.runAsync
  
  module UpdateProjectStatus =
    
    [<Fact>]
    let ``PATCH /api/projects/{id}/status should return 204 NoContent for valid transition`` () =
      async {
        use factory = new TestWebApplicationFactory()
        let project = TestDataGenerators.Bogus.Default.projectWithStatus Idea
        let mockRepo = Fixtures.Mocks.ConfigurableMockRepository()
        mockRepo.SetProjects([project])
        use client = ApiTestHelpers.createClientWithMockRepo mockRepo factory
        
        let request = ApiTestHelpers.updateStatusRequest "inProgress"
        let content = ApiTestHelpers.createJsonContent request
        let! response = client |> ApiTestHelpers.patch $"/api/projects/{project.Id}/status" content
        
        ApiTestHelpers.shouldBeNoContent response
        
        // Verify the update
        let! getResponse = client |> ApiTestHelpers.get $"/api/projects/{project.Id}"
        let getContent = ApiTestHelpers.getResponseContent getResponse
        let updatedProject = getContent |> ApiTestHelpers.fromJson<Project>
        
        updatedProject.Status |> should equal InProgress
      } |> TestHelpers.runAsync
    
    [<Fact>]
    let ``PATCH /api/projects/{id}/status should allow any status transition`` () =
      async {
        use factory = new TestWebApplicationFactory()
        let project = TestDataGenerators.Bogus.Default.projectWithStatus Idea
        let mockRepo = Fixtures.Mocks.ConfigurableMockRepository()
        mockRepo.SetProjects([project])
        use client = ApiTestHelpers.createClientWithMockRepo mockRepo factory
        
        let request = ApiTestHelpers.updateStatusRequest "completed"
        let content = ApiTestHelpers.createJsonContent request
        let! response = client |> ApiTestHelpers.patch $"/api/projects/{project.Id}/status" content
        
        ApiTestHelpers.shouldBeNoContent response
        
        // Verify the update
        let! getResponse = client |> ApiTestHelpers.get $"/api/projects/{project.Id}"
        let getContent = ApiTestHelpers.getResponseContent getResponse
        let updatedProject = getContent |> ApiTestHelpers.fromJson<Project>
        
        updatedProject.Status |> should equal Completed
      } |> TestHelpers.runAsync
  
  module DeleteProject =
    
    [<Fact>]
    let ``DELETE /api/projects/{id} should return 204 NoContent for any project`` () =
      async {
        use factory = new TestWebApplicationFactory()
        let project = TestDataGenerators.Bogus.Default.project()
        let mockRepo = Fixtures.Mocks.ConfigurableMockRepository()
        mockRepo.SetProjects([project])
        use client = ApiTestHelpers.createClientWithMockRepo mockRepo factory
        
        let! response = client |> ApiTestHelpers.delete $"/api/projects/{project.Id}"
        
        ApiTestHelpers.shouldBeNoContent response
        
        // Verify deletion
        let! getResponse = client |> ApiTestHelpers.get $"/api/projects/{project.Id}"
        ApiTestHelpers.shouldBeNotFound getResponse
      } |> TestHelpers.runAsync
    
    [<Fact>]
    let ``DELETE /api/projects/{id} should work for any status`` () =
      async {
        use factory = new TestWebApplicationFactory()
        let activeProject = TestDataGenerators.Bogus.Default.projectWithStatus InProgress
        let mockRepo = Fixtures.Mocks.ConfigurableMockRepository()
        mockRepo.SetProjects([activeProject])
        use client = ApiTestHelpers.createClientWithMockRepo mockRepo factory
        
        let! response = client |> ApiTestHelpers.delete $"/api/projects/{activeProject.Id}"
        
        ApiTestHelpers.shouldBeNoContent response
      } |> TestHelpers.runAsync
    
    [<Fact>]
    let ``DELETE /api/projects/{id} should return 404 NotFound for non-existent project`` () =
      async {
        use factory = new TestWebApplicationFactory()
        use client = ApiTestHelpers.createClientWithInMemoryData factory
        
        let nonExistentId = Guid.NewGuid()
        let! response = client |> ApiTestHelpers.delete $"/api/projects/{nonExistentId}"
        
        ApiTestHelpers.shouldBeNotFound response
      } |> TestHelpers.runAsync