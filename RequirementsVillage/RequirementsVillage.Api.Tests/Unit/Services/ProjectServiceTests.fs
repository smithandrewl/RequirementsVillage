namespace RequirementsVillage.Api.Tests.Unit.Services

open System
open Xunit
open FsUnit.Xunit
open RequirementsVillage.Shared
open RequirementsVillage.Api.Services
open RequirementsVillage.Api.Persistence
open RequirementsVillage.Api.Tests.Helpers
open RequirementsVillage.Shared.Tests.TestGenerators

/// Unit tests for ProjectService focusing on the Create Project feature
module ProjectServiceTests =
  
  module CreateProject =
    
    [<Fact>]
    let ``CreateProject should create new project with Idea status`` () =
      async {
        let mockRepo = Fixtures.Mocks.ConfigurableMockRepository()
        let service = Fixtures.ServiceFactories.createProjectService mockRepo
        let! result = service.CreateProjectAsync(
          "Test Project",
          "Test Description",
          WebApp
        )
        
        let project = TestHelpers.shouldBeOk result
        project.Status |> should equal Idea
        project.Name |> should equal "Test Project"
        project.Description |> should equal "Test Description"
        project.Category |> should equal WebApp
      } |> TestHelpers.runAsync
    
    [<Fact>]
    let ``CreateProject should set CreatedAt and UpdatedAt to current time`` () =
      async {
        let beforeTime = DateTime.UtcNow
        let mockRepo = Fixtures.Mocks.ConfigurableMockRepository()
        let service = Fixtures.ServiceFactories.createProjectService mockRepo
        
        let! result = service.CreateProjectAsync(
          "Time Test Project",
          "Testing timestamps",
          Library
        )
        let afterTime = DateTime.UtcNow
        
        let project = TestHelpers.shouldBeOk result
        project.CreatedAt |> should be (greaterThanOrEqualTo beforeTime)
        project.CreatedAt |> should be (lessThanOrEqualTo afterTime)
        project.UpdatedAt |> should equal project.CreatedAt
      } |> TestHelpers.runAsync