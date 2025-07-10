namespace RequirementsVillage.Api.Tests.Unit.Services

open System
open Xunit
open FsUnit.Xunit
open FsCheck.Xunit
open RequirementsVillage.Api.Models
open RequirementsVillage.Api.Services
open RequirementsVillage.Api.Tests.Helpers
open FsCheck

module ProjectServiceTests =
  
  module GetAllProjects =
    
    [<Fact>]
    let ``GetAllProjects should return all projects from repository`` () =
      async {
        let context = Fixtures.TestContexts.createTestContext()
        let! result = context.Service.GetAllProjectsAsync()
        
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
        let context = Fixtures.TestContexts.createTestContext()
        let targetId = Fixtures.TestData.testProjectId1
        
        let! result = context.Service.GetProjectByIdAsync(targetId)
        
        let projectOption = TestHelpers.shouldBeOk result
        let project = TestHelpers.shouldBeSome projectOption
        project.Id |> should equal targetId
      } |> TestHelpers.runAsync
    
    [<Fact>]
    let ``GetProjectById should return None for non-existent project`` () =
      async {
        let context = Fixtures.TestContexts.createTestContext()
        let! result = context.Service.GetProjectByIdAsync(Fixtures.TestData.nonExistentId)
        
        let projectOption = TestHelpers.shouldBeOk result
        TestHelpers.shouldBeNone projectOption
      } |> TestHelpers.runAsync
    
    [<Property>]
    let ``GetProjectById should find any created project`` (name: string) (desc: string) =
      (TestHelpers.validProjectName name && TestHelpers.validProjectDescription desc) ==> lazy (
        async {
          let service = Fixtures.ServiceFactories.createInMemoryProjectService()
          
          // Create a project
          let! createResult = service.CreateProjectAsync(name, desc, WebApp)
          let createdProject = TestHelpers.shouldBeOk createResult
          
          // Get it by ID
          let! getResult = service.GetProjectByIdAsync(createdProject.Id)
          let foundProjectOption = TestHelpers.shouldBeOk getResult
          let foundProject = TestHelpers.shouldBeSome foundProjectOption
          
          foundProject.Id = createdProject.Id && foundProject.Name = name && foundProject.Description = desc
        } |> TestHelpers.runAsync
      )
  
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
        project.UpdatedAt |> should equal project.CreatedAt
      } |> TestHelpers.runAsync
    
    [<Property>]
    let ``CreateProject should generate unique IDs`` (count: int) =
      (count > 0 && count < 100) ==> lazy (
        async {
          let service = Fixtures.ServiceFactories.createInMemoryProjectService()
          
          let! projects =
            [1..count]
            |> List.map (fun i ->
              service.CreateProjectAsync($"Project {i}", "Description", WebApp))
            |> Async.Parallel
          
          let ids =
            projects
            |> Array.map TestHelpers.shouldBeOk
            |> Array.map (fun p -> p.Id)
          
          ids |> Array.distinct |> Array.length = ids.Length
        } |> TestHelpers.runAsync
      )
  
  module UpdateProject =
    
    [<Fact>]
    let ``UpdateProject should update existing project fields`` () =
      async {
        let context = Fixtures.TestContexts.createTestContext()
        let originalProject = Fixtures.TestData.sampleProjects.[0]
        
        let updatedProject = {
          originalProject with
            Name        = "Updated Name"
            Description = "Updated Description"
            Category    = Game
        }
        
        let! result = context.Service.UpdateProjectAsync(updatedProject)
        
        match result with
        | Ok _ -> ()
        | Error e -> failwithf "Expected Ok but got Error: %A" e
        
        // Verify the update
        let! getResult = context.Service.GetProjectByIdAsync(originalProject.Id)
        let retrieved = TestHelpers.shouldBeSome (TestHelpers.shouldBeOk getResult)
        
        retrieved.Name        |> should equal "Updated Name"
        retrieved.Description |> should equal "Updated Description"
        retrieved.Category    |> should equal Game
      } |> TestHelpers.runAsync
    
    [<Fact>]
    let ``UpdateProject should update UpdatedAt timestamp`` () =
      async {
        let mockRepo = Fixtures.Mocks.ConfigurableMockRepository()
        let oldProject = { Generators.Bogus.project() with UpdatedAt = TestHelpers.yesterday }
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