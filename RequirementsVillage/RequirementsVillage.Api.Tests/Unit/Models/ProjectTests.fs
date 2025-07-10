namespace RequirementsVillage.Api.Tests.Unit.Models

open System
open System.Text.Json
open Xunit
open FsUnit.Xunit
open FsCheck
open FsCheck.Xunit
open RequirementsVillage.Api.Models
open RequirementsVillage.Api.Models.Serialization
open RequirementsVillage.Api.Tests.Helpers

module ProjectTests =
  
  [<Fact>]
  let ``Project should have all required fields`` () =
    let project = TestHelpers.createTestProject()
    
    project.Id          |> should not' (equal Guid.Empty)
    project.Name        |> should not' (be NullOrEmptyString)
    project.Description |> should not' (be NullOrEmptyString)
    project.Category    |> should not' (be null)
    project.Status      |> should not' (be null)
    project.CreatedAt   |> should be (greaterThan DateTime.MinValue)
    project.UpdatedAt   |> should be (greaterThan DateTime.MinValue)
  
  [<Fact>]
  let ``UpdatedAt should be greater than or equal to CreatedAt`` () =
    let project = TestHelpers.createTestProject()
    
    project.UpdatedAt |> should be (greaterThanOrEqualTo project.CreatedAt)
  
  [<Fact>]
  let ``Project serialization should include all fields`` () =
    
    let project = {
      Id          = Guid.Parse("12345678-1234-1234-1234-123456789012")
      Name        = "Test Project"
      Description = "Test Description"
      Category    = WebApp
      Status      = Idea
      CreatedAt   = DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
      UpdatedAt   = DateTime(2024, 1, 2, 0, 0, 0, DateTimeKind.Utc)
    }
    
    let json = JsonSerializer.Serialize(project, jsonOptions)
    
    json |> should haveSubstring "\"id\""
    json |> should haveSubstring "\"name\""
    json |> should haveSubstring "\"description\""
    json |> should haveSubstring "\"category\""
    json |> should haveSubstring "\"status\""
    json |> should haveSubstring "\"createdAt\""
    json |> should haveSubstring "\"updatedAt\""
  
  [<Property>]
  let ``Project serialization round trip should preserve all data``
    (id: Guid)
    (name: string)
    (description: string)
    (category: ProjectCategory)
    (status: ProjectStatus) =
    
    (name <> null && description <> null && TestHelpers.validProjectName name && TestHelpers.validProjectDescription description) ==> lazy (
      
      let project = {
        Id          = id
        Name        = name
        Description = description
        Category    = category
        Status      = status
        CreatedAt   = DateTime.UtcNow
        UpdatedAt   = DateTime.UtcNow.AddHours(1.0)
      }
      
      let json = JsonSerializer.Serialize(project, jsonOptions)
      let deserialized = JsonSerializer.Deserialize<Project>(json, jsonOptions)
      
      deserialized.Id          = project.Id &&
      deserialized.Name        = project.Name &&
      deserialized.Description = project.Description &&
      deserialized.Category    = project.Category &&
      deserialized.Status      = project.Status
    )
  
  module ProjectValidation =
    
    [<Fact>]
    let ``Project name should not be empty`` () =
      let createProjectWithName name = { TestHelpers.createTestProject() with Name = name }
      
      let invalidNames = [ ""; "   "; null ]
      
      invalidNames
      |> List.iter (fun name ->
        let project = createProjectWithName name
        TestHelpers.validProjectName project.Name |> should equal false
      )
    
    [<Fact>]
    let ``Project name should not exceed 100 characters`` () =
      let longName = String.replicate 101 "a"
      let project = { TestHelpers.createTestProject() with Name = longName }
      
      TestHelpers.validProjectName project.Name |> should equal false
    
    [<Fact>]
    let ``Project description should not be empty`` () =
      let createProjectWithDescription desc = 
        { TestHelpers.createTestProject() with Description = desc }
      
      let invalidDescriptions = [ ""; "   "; null ]
      
      invalidDescriptions
      |> List.iter (fun desc ->
        let project = createProjectWithDescription desc
        TestHelpers.validProjectDescription project.Description |> should equal false
      )
    
    [<Fact>]
    let ``Project description should not exceed 1000 characters`` () =
      let longDescription = String.replicate 1001 "a"
      let project = { TestHelpers.createTestProject() with Description = longDescription }
      
      TestHelpers.validProjectDescription project.Description |> should equal false
    
    [<Property>]
    let ``Valid project should pass all validation rules`` (project: Project) =
      Generators.FsCheck.registerGenerators() |> ignore
      
      // Filter out projects with null values
      (project.Name <> null && project.Description <> null) ==> lazy (
        TestHelpers.validProjectName project.Name &&
        TestHelpers.validProjectDescription project.Description
      )
  
  module ProjectEquality =
    
    [<Fact>]
    let ``Projects with same ID should be considered equal`` () =
      let id = Guid.NewGuid()
      let project1 = { TestHelpers.createTestProject() with Id = id; Name = "Project 1" }
      let project2 = { TestHelpers.createTestProject() with Id = id; Name = "Project 2" }
      
      (project1.Id = project2.Id) |> should equal true
    
    [<Fact>]
    let ``Projects with different IDs should not be equal`` () =
      let project1 = TestHelpers.createTestProject()
      let project2 = { TestHelpers.createTestProject() with Id = Guid.NewGuid() }
      
      (project1.Id = project2.Id) |> should equal false
  
  module BogusGeneration =
    
    [<Fact>]
    let ``Bogus should generate valid projects`` () =
      let projects = Generators.Bogus.projects 10
      
      projects |> should haveLength 10
      
      projects
      |> List.iter (fun project ->
        TestHelpers.validProjectName project.Name |> should equal true
        TestHelpers.validProjectDescription project.Description |> should equal true
        project.UpdatedAt |> should be (greaterThanOrEqualTo project.CreatedAt)
      )
    
    [<Fact>]
    let ``Bogus should generate projects with specific status`` () =
      let abandonedProject = Generators.Bogus.projectWithStatus Abandoned
      
      abandonedProject.Status |> should equal Abandoned
    
    [<Fact>]
    let ``Bogus should generate recent projects`` () =
      let recentProject = Generators.Bogus.recentProject()
      let daysSinceCreation = (DateTime.UtcNow - recentProject.CreatedAt).TotalDays
      
      daysSinceCreation |> should be (lessThan 7.0)