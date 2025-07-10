namespace RequirementsVillage.Api.Tests.Unit.Services

open System
open Xunit
open FsUnit.Xunit
open FsCheck.Xunit
open RequirementsVillage.Api.Models
open RequirementsVillage.Api.Services
open RequirementsVillage.Api.Tests.Helpers

module ProjectServiceValidationTests =
  
  let service = Fixtures.ServiceFactories.createSuccessfulProjectService()
  
  module NameValidation =
    
    [<Fact>]
    let ``CreateProject should fail with empty name`` () =
      async {
        let! result = service.CreateProjectAsync("", "Valid description", WebApp)
        
        let error = TestHelpers.shouldBeError result
        TestHelpers.isValidationError error |> should equal true
        
        match error with
        | ValidationFailed(field, reason, _) ->
          field |> should equal "name"
          reason |> should haveSubstring "empty"
        | _ -> failwith "Expected ValidationFailed error"
      } |> TestHelpers.runAsync
    
    [<Fact>]
    let ``CreateProject should fail with whitespace-only name`` () =
      async {
        let! result = service.CreateProjectAsync("   ", "Valid description", WebApp)
        
        let error = TestHelpers.shouldBeError result
        TestHelpers.isValidationError error |> should equal true
      } |> TestHelpers.runAsync
    
    [<Fact>]
    let ``CreateProject should fail with name exceeding 100 characters`` () =
      async {
        let longName = String.replicate 101 "a"
        let! result = service.CreateProjectAsync(longName, "Valid description", WebApp)
        
        let error = TestHelpers.shouldBeError result
        
        match error with
        | ValidationFailed(field, reason, attemptedValue) ->
          field |> should equal "name"
          reason |> should haveSubstring "100 characters"
          attemptedValue.ToString().Length |> should equal 101
        | _ -> failwith "Expected ValidationFailed error"
      } |> TestHelpers.runAsync
    
    [<Property>]
    let ``CreateProject should accept valid names`` (name: string) =
      (TestHelpers.validProjectName name) ==> lazy (
        async {
          let! result = service.CreateProjectAsync(name, "Valid description", WebApp)
          
          match result with
          | Ok project -> project.Name = name
          | Error _    -> false
        } |> TestHelpers.runAsync
      )
  
  module DescriptionValidation =
    
    [<Fact>]
    let ``CreateProject should fail with empty description`` () =
      async {
        let! result = service.CreateProjectAsync("Valid name", "", WebApp)
        
        let error = TestHelpers.shouldBeError result
        TestHelpers.isValidationError error |> should equal true
        
        match error with
        | ValidationFailed(field, reason, _) ->
          field |> should equal "description"
          reason |> should haveSubstring "empty"
        | _ -> failwith "Expected ValidationFailed error"
      } |> TestHelpers.runAsync
    
    [<Fact>]
    let ``CreateProject should fail with description exceeding 1000 characters`` () =
      async {
        let longDescription = String.replicate 1001 "a"
        let! result = service.CreateProjectAsync("Valid name", longDescription, WebApp)
        
        let error = TestHelpers.shouldBeError result
        
        match error with
        | ValidationFailed(field, reason, attemptedValue) ->
          field |> should equal "description"
          reason |> should haveSubstring "1000 characters"
          attemptedValue.ToString().Length |> should equal 1001
        | _ -> failwith "Expected ValidationFailed error"
      } |> TestHelpers.runAsync
    
    [<Property>]
    let ``CreateProject should accept valid descriptions`` (desc: string) =
      (TestHelpers.validProjectDescription desc) ==> lazy (
        async {
          let! result = service.CreateProjectAsync("Valid name", desc, WebApp)
          
          match result with
          | Ok project -> project.Description = desc
          | Error _    -> false
        } |> TestHelpers.runAsync
      )
  
  module StatusTransitionValidation =
    
    [<Fact>]
    let ``UpdateProjectStatus should fail for invalid transition Idea to Completed`` () =
      async {
        // First create a project in Idea status
        let! createResult = service.CreateProjectAsync("Test", "Description", WebApp)
        let project = TestHelpers.shouldBeOk createResult
        
        // Try to transition directly to Completed
        let! updateResult = service.UpdateProjectStatusAsync(project.Id, Completed)
        
        let error = TestHelpers.shouldBeError updateResult
        
        match error with
        | ValidationFailed(field, reason, attemptedValue) ->
          field |> should equal "status"
          reason |> should haveSubstring "Cannot transition directly from Idea to Completed"
          attemptedValue |> should equal Completed
        | _ -> failwith "Expected ValidationFailed error"
      } |> TestHelpers.runAsync
    
    [<Theory>]
    [<InlineData("Idea", "InProgress")>]
    [<InlineData("Idea", "Abandoned")>]
    [<InlineData("Idea", "OnHold")>]
    [<InlineData("InProgress", "Completed")>]
    [<InlineData("InProgress", "Abandoned")>]
    [<InlineData("InProgress", "OnHold")>]
    [<InlineData("OnHold", "InProgress")>]
    [<InlineData("OnHold", "Abandoned")>]
    let ``UpdateProjectStatus should allow valid transitions`` (fromStr: string, toStr: string) =
      async {
        let parseStatus s =
          match s with
          | "Idea"       -> Idea
          | "InProgress" -> InProgress
          | "Completed"  -> Completed
          | "Abandoned"  -> Abandoned
          | "OnHold"     -> OnHold
          | _            -> failwithf "Unknown status: %s" s
        
        let fromStatus = parseStatus fromStr
        let toStatus = parseStatus toStr
        
        // Create project with specific status
        let project = Generators.Bogus.projectWithStatus fromStatus
        let mockRepo = Fixtures.Mocks.ConfigurableMockRepository()
        mockRepo.SetProjects([project])
        let testService = Fixtures.ServiceFactories.createProjectService mockRepo
        
        // Try the transition
        let! result = testService.UpdateProjectStatusAsync(project.Id, toStatus)
        
        result |> should be (ofCase <@ Ok @>)
      } |> TestHelpers.runAsync
  
  module DeleteValidation =
    
    [<Fact>]
    let ``DeleteProject should only allow deletion of Abandoned projects`` () =
      async {
        let nonAbandonedStatuses = [ Idea; InProgress; Completed; OnHold ]
        
        for status in nonAbandonedStatuses do
          let project = Generators.Bogus.projectWithStatus status
          let mockRepo = Fixtures.Mocks.ConfigurableMockRepository()
          mockRepo.SetProjects([project])
          let testService = Fixtures.ServiceFactories.createProjectService mockRepo
          
          let! result = testService.DeleteProjectAsync(project.Id)
          
          let error = TestHelpers.shouldBeError result
          
          match error with
          | ValidationFailed(field, reason, attemptedValue) ->
            field |> should equal "status"
            reason |> should haveSubstring "Can only delete projects in Abandoned status"
            attemptedValue |> should equal status
          | _ -> failwithf "Expected ValidationFailed error for status %A" status
      } |> TestHelpers.runAsync
    
    [<Fact>]
    let ``DeleteProject should succeed for Abandoned projects`` () =
      async {
        let abandonedProject = Generators.Bogus.projectWithStatus Abandoned
        let mockRepo = Fixtures.Mocks.ConfigurableMockRepository()
        mockRepo.SetProjects([abandonedProject])
        let testService = Fixtures.ServiceFactories.createProjectService mockRepo
        
        let! result = testService.DeleteProjectAsync(abandonedProject.Id)
        
        result |> should be (ofCase <@ Ok @>)
      } |> TestHelpers.runAsync
    
    [<Fact>]
    let ``DeleteProject should fail for non-existent project`` () =
      async {
        let! result = service.DeleteProjectAsync(Fixtures.TestData.nonExistentId)
        
        let error = TestHelpers.shouldBeError result
        TestHelpers.isNotFoundError error |> should equal true
      } |> TestHelpers.runAsync