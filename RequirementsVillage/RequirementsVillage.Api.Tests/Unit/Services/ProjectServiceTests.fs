namespace RequirementsVillage.Api.Tests.Unit.Services

open System
open Xunit
open FsUnit.Xunit
open FsCheck.Xunit
open RequirementsVillage.Shared
open RequirementsVillage.Api.Services
open RequirementsVillage.Api.Persistence
open RequirementsVillage.Api.Tests.Helpers
open RequirementsVillage.Shared.TestGenerators
open FsCheck
open NSubstitute

module ProjectServiceTests =
  
  module GetAllProjects =
    
    [<Fact>]
    let ``GetAllProjects should return all projects from repository`` () =
      async {
        let mockRepo = Fixtures.Mocks.ConfigurableMockRepository()
        mockRepo.SetProjects(Fixtures.TestData.sampleProjects)
        let service = Fixtures.ServiceFactories.createProjectService mockRepo
        let! result = service.GetAllProjectsAsync()
        
        let projects = TestHelpers.shouldBeOk result
        projects |> should not' (be Empty)
      } |> TestHelpers.runAsync
    
    [<Fact>]
    let ``GetAllProjects should return empty list when no projects exist`` () =
      async {
        let context = Fixtures.TestContexts.createEmptyTestContext()
        let! result = context.Service.GetAllProjectsAsync()
        
        let projects = TestHelpers.shouldBeOk result
        projects |> should be Empty
      } |> TestHelpers.runAsync
    
    [<Fact>]
    let ``GetAllProjects should handle repository errors`` () =
      async {
        let error = DatabaseError("SELECT", "Projects", Exception("Connection failed"))
        let service = Fixtures.ServiceFactories.createFailingProjectService error
        
        let! result = service.GetAllProjectsAsync()
        
        let actualError = TestHelpers.shouldBeError result
        TestHelpers.isDatabaseError actualError |> should equal true
      } |> TestHelpers.runAsync
  
  module GetProjectById =
    
    [<Fact>]
    let ``GetProjectById should return existing project`` () =
      async {
        // Use configurable mock repository with test data
        let mockRepo = Fixtures.Mocks.ConfigurableMockRepository()
        mockRepo.SetProjects(Fixtures.TestData.sampleProjects)
        let service = Fixtures.ServiceFactories.createProjectService mockRepo
        let targetId = Fixtures.TestData.testProjectId1
        
        let! result = service.GetProjectByIdAsync(targetId)
        
        let projectOption = TestHelpers.shouldBeOk result
        let project = TestHelpers.shouldBeSome projectOption
        project.Id |> should equal targetId
      } |> TestHelpers.runAsync
    
    [<Fact>]
    let ``GetProjectById should return None for non-existent project`` () =
      async {
        let mockRepo = Fixtures.Mocks.ConfigurableMockRepository()
        mockRepo.SetProjects(Fixtures.TestData.sampleProjects)
        let service = Fixtures.ServiceFactories.createProjectService mockRepo
        let! result = service.GetProjectByIdAsync(Fixtures.TestData.nonExistentId)
        
        let projectOption = TestHelpers.shouldBeOk result
        TestHelpers.shouldBeNone projectOption
      } |> TestHelpers.runAsync
    
  
  module CreateProject =
    
    [<Fact>]
    let ``CreateProject should create new project with Idea status`` () =
      async {
        let service = Fixtures.ServiceFactories.createInMemoryProjectService()
        let name = "New Project"
        let description = "Project description"
        let category = Library
        
        let! result = service.CreateProjectAsync(name, description, category)
        
        let project = TestHelpers.shouldBeOk result
        project.Name        |> should equal name
        project.Description |> should equal description
        project.Category    |> should equal category
        project.Status      |> should equal Idea
        project.Id          |> should not' (equal Guid.Empty)
      } |> TestHelpers.runAsync
    
    [<Fact>]
    let ``CreateProject should set CreatedAt and UpdatedAt to current time`` () =
      async {
        let service = Fixtures.ServiceFactories.createInMemoryProjectService()
        let beforeCreate = DateTime.UtcNow
        
        let! result = service.CreateProjectAsync("Test", "Description", Tool)
        
        let project = TestHelpers.shouldBeOk result
        let afterCreate = DateTime.UtcNow
        
        project.CreatedAt |> should be (greaterThanOrEqualTo beforeCreate)
        project.CreatedAt |> should be (lessThanOrEqualTo afterCreate)
        // UpdatedAt should be very close to CreatedAt (within a millisecond)
        let timeDiff = abs(project.UpdatedAt.Subtract(project.CreatedAt).TotalMilliseconds)
        timeDiff |> should be (lessThan 1.0)
      } |> TestHelpers.runAsync
    
  
  module UpdateProject =
    
    [<Fact>]
    let ``UpdateProject should update existing project fields`` () =
      async {
        // Use configurable mock repository with test data
        let mockRepo = Fixtures.Mocks.ConfigurableMockRepository()
        mockRepo.SetProjects(Fixtures.TestData.sampleProjects)
        let service = Fixtures.ServiceFactories.createProjectService mockRepo
        
        let originalProject = Fixtures.TestData.sampleProjects.[0]
        
        let updatedProject = {
          originalProject with
            Name        = "Updated Name"
            Description = "Updated Description"
            Category    = Game
        }
        
        let! result = service.UpdateProjectAsync(updatedProject)
        
        match result with
        | Ok _ -> ()
        | Error e -> failwithf "Expected Ok but got Error: %A" e
        
        // Verify the update
        let! getResult = service.GetProjectByIdAsync(originalProject.Id)
        let retrieved = TestHelpers.shouldBeSome (TestHelpers.shouldBeOk getResult)
        
        retrieved.Name        |> should equal "Updated Name"
        retrieved.Description |> should equal "Updated Description"
        retrieved.Category    |> should equal Game
      } |> TestHelpers.runAsync
    
    [<Fact>]
    let ``UpdateProject should update UpdatedAt timestamp`` () =
      async {
        let mockRepo = Fixtures.Mocks.ConfigurableMockRepository()
        let oldProject = { TestDataGenerators.Bogus.Default.project() with UpdatedAt = TestHelpers.yesterday }
        mockRepo.SetProjects([oldProject])
        let service = Fixtures.ServiceFactories.createProjectService mockRepo
        
        let beforeUpdate = DateTime.UtcNow
        let! result = service.UpdateProjectAsync(oldProject)
        let afterUpdate = DateTime.UtcNow
        
        match result with
        | Ok _ -> ()
        | Error e -> failwithf "Expected Ok but got Error: %A" e
        
        let! getResult = service.GetProjectByIdAsync(oldProject.Id)
        let updated = TestHelpers.shouldBeSome (TestHelpers.shouldBeOk getResult)
        
        updated.UpdatedAt |> should be (greaterThan oldProject.UpdatedAt)
        updated.UpdatedAt |> should be (greaterThanOrEqualTo beforeUpdate)
        updated.UpdatedAt |> should be (lessThanOrEqualTo afterUpdate)
      } |> TestHelpers.runAsync
    
    [<Fact>]
    let ``UpdateProject should fail for non-existent project`` () =
      async {
        let context = Fixtures.TestContexts.createTestContext()
        let nonExistentProject = {
          TestHelpers.createTestProject() with
            Id = Fixtures.TestData.nonExistentId
        }
        
        let! result = context.Service.UpdateProjectAsync(nonExistentProject)
        
        let error = TestHelpers.shouldBeError result
        TestHelpers.isNotFoundError error |> should equal true
      } |> TestHelpers.runAsync
  
  module ErrorHandling =
    
    [<Fact>]
    let ``Service should propagate repository errors for GetAll`` () =
      async {
        let error = DatabaseError("GetAll", "Projects", Exception("Test error"))
        let service = Fixtures.ServiceFactories.createFailingProjectService error
        
        let! result = service.GetAllProjectsAsync()
        
        let actualError = TestHelpers.shouldBeError result
        
        match actualError with
        | DatabaseError(op, table, _) ->
          op    |> should equal "GetAll"
          table |> should equal "Projects"
        | _ -> failwith "Expected DatabaseError"
      } |> TestHelpers.runAsync
    
    [<Fact>]
    let ``Service should propagate repository errors for GetById`` () =
      async {
        let error = DatabaseError("GetById", "Projects", Exception("Test error"))
        let service = Fixtures.ServiceFactories.createFailingProjectService error
        let testId = Guid.NewGuid()
        
        let! result = service.GetProjectByIdAsync(testId)
        
        let actualError = TestHelpers.shouldBeError result
        
        match actualError with
        | DatabaseError(op, table, _) ->
          op    |> should equal "GetById"
          table |> should equal "Projects"
        | _ -> failwith "Expected DatabaseError"
      } |> TestHelpers.runAsync
    
    [<Fact>]
    let ``Service should handle concurrent operations safely`` () =
      async {
        let mockRepo = Substitute.For<IProjectRepository>()
        mockRepo.CreateAsync(Arg.Any<Project>())
          .Returns(async { return Ok () }) |> ignore
        
        let service = ProjectService(mockRepo) :> IProjectService
        
        // Create multiple projects concurrently
        let! results =
          [1..10]
          |> List.map (fun i ->
            service.CreateProjectAsync($"Project {i}", "Description", WebApp))
          |> Async.Parallel
        
        // All should succeed
        results |> Array.iter (fun result ->
          match result with
          | Ok _ -> ()
          | Error e -> failwithf "Expected Ok but got Error: %A" e
        )
        
        // Verify repository was called 10 times
        mockRepo.Received(10).CreateAsync(Arg.Any<Project>()) |> ignore
      } |> TestHelpers.runAsync
  
  module UpdateProjectStatus =
    
    [<Fact>]
    let ``UpdateProjectStatus should update status of existing project`` () =
      async {
        let projectId = Guid.NewGuid()
        let existingProject = {
          TestDataGenerators.Bogus.Default.project() with
            Id     = projectId
            Status = Idea
        }
        
        let mockRepo = Substitute.For<IProjectRepository>()
        mockRepo.GetByIdAsync(projectId)
          .Returns(async { return Ok (Some existingProject) }) |> ignore
        mockRepo.UpdateAsync(Arg.Any<Project>())
          .Returns(async { return Ok () }) |> ignore
        
        let service = ProjectService(mockRepo) :> IProjectService
        
        let! result = service.UpdateProjectStatusAsync(projectId, InProgress)
        
        match result with
        | Ok _ ->
          // Verify the repository was called with correct status
          mockRepo.Received(1).UpdateAsync(
            Arg.Is<Project>(fun p -> 
              p.Id = projectId && p.Status = InProgress
            )
          ) |> ignore
        | Error e -> failwithf "Expected Ok but got Error: %A" e
      } |> TestHelpers.runAsync
    
    [<Fact>]
    let ``UpdateProjectStatus should fail for non-existent project`` () =
      async {
        let projectId = Guid.NewGuid()
        
        let mockRepo = Substitute.For<IProjectRepository>()
        mockRepo.GetByIdAsync(projectId)
          .Returns(async { return Ok None }) |> ignore
        
        let service = ProjectService(mockRepo) :> IProjectService
        
        let! result = service.UpdateProjectStatusAsync(projectId, InProgress)
        
        let error = TestHelpers.shouldBeError result
        TestHelpers.isNotFoundError error |> should equal true
        
        // Verify UpdateAsync was not called
        mockRepo.DidNotReceive().UpdateAsync(Arg.Any<Project>()) |> ignore
      } |> TestHelpers.runAsync
    
  
  module DeleteProject =
    
    [<Fact>]
    let ``DeleteProject should delete any project`` () =
      async {
        let projectId = Guid.NewGuid()
        let project = { TestDataGenerators.Bogus.Default.project() with Id = projectId }
        
        let mockRepo = Substitute.For<IProjectRepository>()
        mockRepo.GetByIdAsync(projectId)
          .Returns(async { return Ok (Some project) }) |> ignore
        mockRepo.DeleteAsync(projectId)
          .Returns(async { return Ok () }) |> ignore
        
        let service = ProjectService(mockRepo) :> IProjectService
        
        let! result = service.DeleteProjectAsync(projectId)
        
        match result with
        | Ok _ ->
          // Verify both repository methods were called
          mockRepo.Received(1).GetByIdAsync(projectId) |> ignore
          mockRepo.Received(1).DeleteAsync(projectId) |> ignore
        | Error e -> failwithf "Expected Ok but got Error: %A" e
      } |> TestHelpers.runAsync
    
    
    [<Fact>]
    let ``DeleteProject should handle repository errors`` () =
      async {
        let projectId = Guid.NewGuid()
        let project = TestDataGenerators.Bogus.Default.project()
        
        let mockRepo = Substitute.For<IProjectRepository>()
        mockRepo.GetByIdAsync(projectId)
          .Returns(async { return Ok (Some project) }) |> ignore
        mockRepo.DeleteAsync(projectId)
          .Returns(async { 
            return Error (DatabaseError("DELETE", "Projects", Exception("Database error"))) 
          }) |> ignore
        
        let service = ProjectService(mockRepo) :> IProjectService
        
        let! result = service.DeleteProjectAsync(projectId)
        
        let error = TestHelpers.shouldBeError result
        TestHelpers.isDatabaseError error |> should equal true
      } |> TestHelpers.runAsync
  
  module RepositoryInteraction =
    
    [<Fact>]
    let ``CreateProject should call repository with correct project data`` () =
      async {
        let mockRepo = Substitute.For<IProjectRepository>()
        mockRepo.CreateAsync(Arg.Any<Project>())
          .Returns(async { return Ok () }) |> ignore
        
        let service = ProjectService(mockRepo) :> IProjectService
        
        let name        = "Test Project"
        let description = "Test Description"
        let category    = WebApp
        
        let! result = service.CreateProjectAsync(name, description, category)
        
        match result with
        | Ok project ->
          // Verify repository was called with correct data
          mockRepo.Received(1).CreateAsync(
            Arg.Is<Project>(fun (p: Project) -> 
              p.Name = name &&
              p.Description = description &&
              p.Category = category &&
              p.Status = Idea &&
              p.Id <> Guid.Empty
            )
          ) |> ignore
        | Error e -> failwithf "Expected Ok but got Error: %A" e
      } |> TestHelpers.runAsync
    
    [<Fact>]
    let ``UpdateProject should preserve CreatedAt and update UpdatedAt`` () =
      async {
        let projectId = Guid.NewGuid()
        let originalCreatedAt = DateTime.UtcNow.AddDays(-7.0)
        let originalProject = {
          TestDataGenerators.Bogus.Default.project() with
            Id        = projectId
            CreatedAt = originalCreatedAt
            UpdatedAt = originalCreatedAt
        }
        
        let mockRepo = Substitute.For<IProjectRepository>()
        mockRepo.GetByIdAsync(projectId)
          .Returns(async { return Ok (Some originalProject) }) |> ignore
        mockRepo.UpdateAsync(Arg.Any<Project>())
          .Returns(async { return Ok () }) |> ignore
        
        let service = ProjectService(mockRepo) :> IProjectService
        
        let updatedProject = {
          originalProject with
            Name = "Updated Name"
        }
        
        let beforeUpdate = DateTime.UtcNow
        let! result = service.UpdateProjectAsync(updatedProject)
        let afterUpdate = DateTime.UtcNow
        
        match result with
        | Ok _ ->
          // Verify the repository was called with preserved CreatedAt
          mockRepo.Received(1).UpdateAsync(
            Arg.Is<Project>(fun p -> 
              p.CreatedAt = originalCreatedAt &&
              p.UpdatedAt >= beforeUpdate &&
              p.UpdatedAt <= afterUpdate
            )
          ) |> ignore
        | Error e -> failwithf "Expected Ok but got Error: %A" e
      } |> TestHelpers.runAsync