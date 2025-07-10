namespace RequirementsVillage.Api.Tests.Helpers

open System
open RequirementsVillage.Api.Models
open RequirementsVillage.Api.Persistence
open RequirementsVillage.Api.Services

module Fixtures =
  
  // Common test data
  module TestData =
    
    let testProjectId1 = Guid.Parse("11111111-1111-1111-1111-111111111111")
    let testProjectId2 = Guid.Parse("22222222-2222-2222-2222-222222222222")
    let testProjectId3 = Guid.Parse("33333333-3333-3333-3333-333333333333")
    let nonExistentId  = Guid.Parse("99999999-9999-9999-9999-999999999999")
    
    let baseDate = DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    
    let sampleProjects = [
      { Id          = testProjectId1
        Name        = "Test Web App"
        Description = "A sample web application for testing"
        Category    = WebApp
        Status      = Idea
        CreatedAt   = baseDate
        UpdatedAt   = baseDate }
      
      { Id          = testProjectId2
        Name        = "Mobile Testing Tool"
        Description = "A mobile app for automated testing"
        Category    = MobileApp
        Status      = InProgress
        CreatedAt   = baseDate.AddDays(-30.0)
        UpdatedAt   = baseDate.AddDays(-1.0) }
      
      { Id          = testProjectId3
        Name        = "Abandoned Library"
        Description = "A library project that was abandoned"
        Category    = Library
        Status      = Abandoned
        CreatedAt   = baseDate.AddDays(-90.0)
        UpdatedAt   = baseDate.AddDays(-60.0) }
    ]
    
    let emptyProject = {
      Id          = Guid.Empty
      Name        = ""
      Description = ""
      Category    = WebApp
      Status      = Idea
      CreatedAt   = DateTime.MinValue
      UpdatedAt   = DateTime.MinValue
    }
    
    let longName = String.replicate 101 "a"
    let longDescription = String.replicate 1001 "a"
    
    let validName = "Valid Project Name"
    let validDescription = "This is a valid project description."
  
  // Test doubles and mocks
  module Mocks =
    
    // A simple mock repository that always returns success
    type SuccessfulMockRepository() =
      interface IProjectRepository with
        member _.GetAllAsync() =
          async { return Ok TestData.sampleProjects }
        
        member _.GetByIdAsync(id) =
          async {
            let project = TestData.sampleProjects |> List.tryFind (fun p -> p.Id = id)
            return Ok project
          }
        
        member _.CreateAsync(project) =
          async { return Ok () }
        
        member _.UpdateAsync(project) =
          async { return Ok () }
        
        member _.DeleteAsync(id) =
          async { return Ok () }
    
    // A mock repository that always returns errors
    type FailingMockRepository(error: ProjectError) =
      interface IProjectRepository with
        member _.GetAllAsync() =
          async { return Error error }
        
        member _.GetByIdAsync(id) =
          async { return Error error }
        
        member _.CreateAsync(project) =
          async { return Error error }
        
        member _.UpdateAsync(project) =
          async { return Error error }
        
        member _.DeleteAsync(id) =
          async { return Error error }
    
    // A configurable mock repository for specific test scenarios
    type ConfigurableMockRepository() =
      let mutable projects = TestData.sampleProjects
      
      member _.SetProjects(newProjects) =
        projects <- newProjects
      
      interface IProjectRepository with
        member _.GetAllAsync() =
          async { return Ok projects }
        
        member _.GetByIdAsync(id) =
          async {
            let project = projects |> List.tryFind (fun p -> p.Id = id)
            return Ok project
          }
        
        member _.CreateAsync(project) =
          async {
            projects <- project :: projects
            return Ok ()
          }
        
        member _.UpdateAsync(project) =
          async {
            match projects |> List.tryFindIndex (fun p -> p.Id = project.Id) with
            | Some idx ->
              projects <- projects |> List.updateAt idx project
              return Ok ()
            | None ->
              return Error (NotFound(project.Id, "ConfigurableMockRepository"))
          }
        
        member _.DeleteAsync(id) =
          async {
            projects <- projects |> List.filter (fun p -> p.Id <> id)
            return Ok ()
          }
  
  // Factory methods for creating test services
  module ServiceFactories =
    
    let createProjectService (repository: IProjectRepository) =
      ProjectService(repository) :> IProjectService
    
    let createSuccessfulProjectService() =
      createProjectService (Mocks.SuccessfulMockRepository())
    
    let createFailingProjectService (error: ProjectError) =
      createProjectService (Mocks.FailingMockRepository(error))
    
    let createInMemoryProjectService() =
      createProjectService (InMemoryProjectRepository())
  
  // Test context builders for integration tests
  module TestContexts =
    
    type TestContext = {
      Repository: IProjectRepository
      Service:    IProjectService
      TestData:   Project list
    }
    
    let createTestContext() =
      let repository = InMemoryProjectRepository()
      let service = ServiceFactories.createProjectService repository
      {
        Repository = repository
        Service    = service
        TestData   = TestData.sampleProjects
      }
    
    let createEmptyTestContext() =
      let repository = Mocks.ConfigurableMockRepository()
      repository.SetProjects([])
      let service = ServiceFactories.createProjectService repository
      {
        Repository = repository
        Service    = service
        TestData   = []
      }