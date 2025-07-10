namespace RequirementsVillage.Api.Tests.Unit.Repository

open System
open Xunit
open FsUnit.Xunit
open RequirementsVillage.Api.Models
open RequirementsVillage.Api.Persistence
open RequirementsVillage.Api.Tests.Helpers

module InMemoryProjectRepositoryTests =
  
  let createRepository() = InMemoryProjectRepository() :> IProjectRepository
  
  module GetAll =
    
    [<Fact>]
    let ``GetAllAsync should return initial sample projects`` () =
      async {
        let repo = createRepository()
        let! result = repo.GetAllAsync()
        
        let projects = TestHelpers.shouldBeOk result
        projects |> should not' (be Empty)
        projects |> List.length |> should be (greaterThan 0)
      } |> TestHelpers.runAsync
    
    [<Fact>]
    let ``GetAllAsync should include newly created projects`` () =
      async {
        let repo = createRepository()
        let newProject = TestHelpers.createTestProject()
        
        let! createResult = repo.CreateAsync(newProject)
        match createResult with
        | Ok _ -> ()
        | Error e -> failwithf "Expected Ok but got Error: %A" e
        
        let! getAllResult = repo.GetAllAsync()
        let projects = TestHelpers.shouldBeOk getAllResult
        
        projects |> TestHelpers.shouldContain newProject
      } |> TestHelpers.runAsync
  
  module GetById =
    
    [<Fact>]
    let ``GetByIdAsync should return existing project`` () =
      async {
        let repo = createRepository()
        let newProject = TestHelpers.createTestProject()
        
        let! createResult = repo.CreateAsync(newProject)
        match createResult with
        | Ok _ -> ()
        | Error e -> failwithf "Expected Ok but got Error: %A" e
        
        let! getResult = repo.GetByIdAsync(newProject.Id)
        let foundProject = TestHelpers.shouldBeSome (TestHelpers.shouldBeOk getResult)
        
        foundProject |> should equal newProject
      } |> TestHelpers.runAsync
    
    [<Fact>]
    let ``GetByIdAsync should return None for non-existent project`` () =
      async {
        let repo = createRepository()
        let! result = repo.GetByIdAsync(Guid.NewGuid())
        
        let projectOption = TestHelpers.shouldBeOk result
        TestHelpers.shouldBeNone projectOption
      } |> TestHelpers.runAsync
  
  module Create =
    
    [<Fact>]
    let ``CreateAsync should add project to repository`` () =
      async {
        let repo = createRepository()
        let newProject = TestHelpers.createTestProject()
        
        let! createResult = repo.CreateAsync(newProject)
        match createResult with
        | Ok _ -> ()
        | Error e -> failwithf "Expected Ok but got Error: %A" e
        
        let! getResult = repo.GetByIdAsync(newProject.Id)
        let foundProject = TestHelpers.shouldBeSome (TestHelpers.shouldBeOk getResult)
        
        foundProject |> should equal newProject
      } |> TestHelpers.runAsync
    
    [<Fact>]
    let ``CreateAsync should maintain project order (newest first)`` () =
      async {
        let repo = createRepository()
        let project1 = { TestHelpers.createTestProject() with Name = "First" }
        let project2 = { TestHelpers.createTestProject() with Name = "Second" }
        let project3 = { TestHelpers.createTestProject() with Name = "Third" }
        
        let! _ = repo.CreateAsync(project1)
        let! _ = repo.CreateAsync(project2)
        let! _ = repo.CreateAsync(project3)
        
        let! getAllResult = repo.GetAllAsync()
        let projects = TestHelpers.shouldBeOk getAllResult
        
        // Newest should be first (prepended to list)
        projects |> List.head |> (fun p -> p.Name) |> should equal "Third"
      } |> TestHelpers.runAsync
  
  module Update =
    
    [<Fact>]
    let ``UpdateAsync should modify existing project`` () =
      async {
        let repo = createRepository()
        let originalProject = TestHelpers.createTestProject()
        
        let! createResult = repo.CreateAsync(originalProject)
        match createResult with
        | Ok _ -> ()
        | Error e -> failwithf "Expected Ok but got Error: %A" e
        
        let updatedProject = {
          originalProject with
            Name = "Updated Name"
            Description = "Updated Description"
            Status = Completed
        }
        
        let! updateResult = repo.UpdateAsync(updatedProject)
        match updateResult with
        | Ok _ -> ()
        | Error e -> failwithf "Expected Ok but got Error: %A" e
        
        let! getResult = repo.GetByIdAsync(originalProject.Id)
        let foundProject = TestHelpers.shouldBeSome (TestHelpers.shouldBeOk getResult)
        
        foundProject.Name        |> should equal "Updated Name"
        foundProject.Description |> should equal "Updated Description"
        foundProject.Status      |> should equal Completed
      } |> TestHelpers.runAsync
    
    [<Fact>]
    let ``UpdateAsync should fail for non-existent project`` () =
      async {
        let repo = createRepository()
        let nonExistentProject = TestHelpers.createTestProject()
        
        let! result = repo.UpdateAsync(nonExistentProject)
        
        let error = TestHelpers.shouldBeError result
        TestHelpers.isNotFoundError error |> should equal true
      } |> TestHelpers.runAsync
    
    [<Fact>]
    let ``UpdateAsync should preserve other projects`` () =
      async {
        let repo = createRepository()
        let project1 = TestHelpers.createTestProject()
        let project2 = TestHelpers.createTestProject()
        
        let! _ = repo.CreateAsync(project1)
        let! _ = repo.CreateAsync(project2)
        
        let updatedProject1 = { project1 with Name = "Updated" }
        let! _ = repo.UpdateAsync(updatedProject1)
        
        let! getAllResult = repo.GetAllAsync()
        let projects = TestHelpers.shouldBeOk getAllResult
        
        projects |> TestHelpers.shouldContain project2
      } |> TestHelpers.runAsync
  
  module Delete =
    
    [<Fact>]
    let ``DeleteAsync should remove project from repository`` () =
      async {
        let repo = createRepository()
        let project = TestHelpers.createTestProject()
        
        let! createResult = repo.CreateAsync(project)
        match createResult with
        | Ok _ -> ()
        | Error e -> failwithf "Expected Ok but got Error: %A" e
        
        let! deleteResult = repo.DeleteAsync(project.Id)
        match deleteResult with
        | Ok _ -> ()
        | Error e -> failwithf "Expected Ok but got Error: %A" e
        
        let! getResult = repo.GetByIdAsync(project.Id)
        let projectOption = TestHelpers.shouldBeOk getResult
        TestHelpers.shouldBeNone projectOption
      } |> TestHelpers.runAsync
    
    [<Fact>]
    let ``DeleteAsync should succeed even for non-existent project`` () =
      async {
        let repo = createRepository()
        let! result = repo.DeleteAsync(Guid.NewGuid())
        
        match result with
        | Ok _ -> ()
        | Error e -> failwithf "Expected Ok but got Error: %A" e
      } |> TestHelpers.runAsync
    
    [<Fact>]
    let ``DeleteAsync should not affect other projects`` () =
      async {
        let repo = createRepository()
        let project1 = TestHelpers.createTestProject()
        let project2 = TestHelpers.createTestProject()
        
        let! _ = repo.CreateAsync(project1)
        let! _ = repo.CreateAsync(project2)
        
        let! _ = repo.DeleteAsync(project1.Id)
        
        let! getAllResult = repo.GetAllAsync()
        let projects = TestHelpers.shouldBeOk getAllResult
        
        projects |> TestHelpers.shouldNotContain project1
        projects |> TestHelpers.shouldContain project2
      } |> TestHelpers.runAsync
  
  module Concurrency =
    
    [<Fact>]
    let ``Repository should handle concurrent creates`` () =
      async {
        let repo = createRepository()
        let projects = Generators.Bogus.projects 10
        
        let! results =
          projects
          |> List.map repo.CreateAsync
          |> Async.Parallel
        
        results |> Array.forall (fun r -> match r with Ok _ -> true | _ -> false)
        |> should equal true
        
        let! getAllResult = repo.GetAllAsync()
        let allProjects = TestHelpers.shouldBeOk getAllResult
        
        projects
        |> List.iter (fun p -> allProjects |> TestHelpers.shouldContain p)
      } |> TestHelpers.runAsync