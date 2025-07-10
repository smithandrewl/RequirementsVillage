namespace RequirementsVillage.Api.Tests.Unit.Services

open System
open Xunit
open FsUnit.Xunit
open FsCheck.Xunit
open RequirementsVillage.Api.Models
open RequirementsVillage.Api.Services
open RequirementsVillage.Api.Persistence
open RequirementsVillage.Api.Tests.Helpers
open FsCheck
open NSubstitute

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
          | Ok project -> 
            project.Name |> should equal name
            true
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
          | Ok project -> 
            project.Description |> should equal desc
            true
          | Error _    -> false
        } |> TestHelpers.runAsync
      )
  
  module StatusTransitionValidation =
    
    [<Fact>]
    let ``UpdateProjectStatus should fail for invalid transition Idea to Completed`` () =
      async {
        // Use in-memory repository so we can actually store and retrieve projects
        let testService = Fixtures.ServiceFactories.createInMemoryProjectService()
        
        // First create a project in Idea status
        let! createResult = testService.CreateProjectAsync("Test", "Description", WebApp)
        let project = TestHelpers.shouldBeOk createResult
        
        // Try to transition directly to Completed
        let! updateResult = testService.UpdateProjectStatusAsync(project.Id, Completed)
        
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
        
        match result with
        | Ok _ -> ()
        | Error e -> failwithf "Expected Ok but got Error: %A" e
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
        
        match result with
        | Ok _ -> ()
        | Error e -> failwithf "Expected Ok but got Error: %A" e
      } |> TestHelpers.runAsync
    
    [<Fact>]
    let ``DeleteProject should fail for non-existent project`` () =
      async {
        let! result = service.DeleteProjectAsync(Fixtures.TestData.nonExistentId)
        
        let error = TestHelpers.shouldBeError result
        TestHelpers.isNotFoundError error |> should equal true
      } |> TestHelpers.runAsync
  
  module ComplexValidationScenarios =
    
    [<Fact>]
    let ``UpdateProject should validate both name and description`` () =
      async {
        let projectId = Guid.NewGuid()
        let existingProject = Generators.Bogus.project()
        
        let mockRepo = Substitute.For<IProjectRepository>()
        mockRepo.GetByIdAsync(projectId)
          .Returns(async { return Ok (Some existingProject) }) |> ignore
        
        let testService = ProjectService(mockRepo) :> IProjectService
        
        // Try update with empty name
        let invalidProject1 = { existingProject with Name = "" }
        let! result1 = testService.UpdateProjectAsync(invalidProject1)
        
        let error1 = TestHelpers.shouldBeError result1
        match error1 with
        | ValidationFailed(field, _, _) -> field |> should equal "name"
        | _ -> failwith "Expected ValidationFailed for name"
        
        // Try update with empty description
        let invalidProject2 = { existingProject with Description = "" }
        let! result2 = testService.UpdateProjectAsync(invalidProject2)
        
        let error2 = TestHelpers.shouldBeError result2
        match error2 with
        | ValidationFailed(field, _, _) -> field |> should equal "description"
        | _ -> failwith "Expected ValidationFailed for description"
        
        // Verify repository update was never called
        mockRepo.DidNotReceive().UpdateAsync(Arg.Any<Project>()) |> ignore
      } |> TestHelpers.runAsync
    
    [<Fact>]
    let ``Service should validate before any repository interaction`` () =
      async {
        let mockRepo = Substitute.For<IProjectRepository>()
        let testService = ProjectService(mockRepo) :> IProjectService
        
        // Create with invalid name
        let! result = testService.CreateProjectAsync("", "Valid desc", WebApp)
        
        TestHelpers.shouldBeError result |> ignore
        
        // Verify no repository methods were called
        mockRepo.DidNotReceive().CreateAsync(Arg.Any<Project>()) |> ignore
        mockRepo.DidNotReceive().GetByIdAsync(Arg.Any<Guid>()) |> ignore
        mockRepo.DidNotReceive().UpdateAsync(Arg.Any<Project>()) |> ignore
        mockRepo.DidNotReceive().DeleteAsync(Arg.Any<Guid>()) |> ignore
      } |> TestHelpers.runAsync
    
    [<Fact>]
    let ``Valid project names should be preserved with whitespace`` () =
      async {
        // Test with various amounts of spacing
        for spaces in [1; 2; 5] do
          let spacesStr = String.replicate spaces " "
          let nameWithSpaces = $"{spacesStr}Valid Name{spacesStr}"
          
          let mockRepo = Substitute.For<IProjectRepository>()
          mockRepo.CreateAsync(Arg.Any<Project>())
            .Returns(async { return Ok () }) |> ignore
          
          let testService = ProjectService(mockRepo) :> IProjectService
          
          let! result = testService.CreateProjectAsync(nameWithSpaces, "Description", Tool)
          
          match result with
          | Ok project -> 
            // Service should NOT trim - it should preserve the input
            project.Name |> should equal nameWithSpaces
          | Error _ -> failwith "Expected success"
      } |> TestHelpers.runAsync
  
  module EdgeCaseValidation =
    
    [<Theory>]
    [<InlineData(null)>]
    [<InlineData("")>]
    [<InlineData("   ")>]
    [<InlineData("\t\n\r")>]
    let ``CreateProject should reject various empty name formats`` (name: string) =
      async {
        let! result = service.CreateProjectAsync(name, "Valid description", WebApp)
        
        let error = TestHelpers.shouldBeError result
        match error with
        | ValidationFailed(field, reason, _) ->
          field |> should equal "name"
          reason |> should haveSubstring "empty"
        | _ -> failwith "Expected ValidationFailed error"
      } |> TestHelpers.runAsync
    
    [<Fact>]
    let ``UpdateProject should handle status transition with other changes`` () =
      async {
        let projectId = Guid.NewGuid()
        let existingProject = {
          Generators.Bogus.project() with
            Id     = projectId
            Status = Idea
        }
        
        let mockRepo = Substitute.For<IProjectRepository>()
        mockRepo.GetByIdAsync(projectId)
          .Returns(async { return Ok (Some existingProject) }) |> ignore
        mockRepo.UpdateAsync(Arg.Any<Project>())
          .Returns(async { return Ok () }) |> ignore
        
        let testService = ProjectService(mockRepo) :> IProjectService
        
        // Valid update with status change
        let updatedProject = {
          existingProject with
            Name   = "New Name"
            Status = InProgress
        }
        
        let! result = testService.UpdateProjectAsync(updatedProject)
        
        match result with
        | Ok _ ->
          // Verify repository was called with all changes
          mockRepo.Received(1).UpdateAsync(
            Arg.Is<Project>(fun (p: Project) ->
              p.Name = "New Name" &&
              p.Status = InProgress &&
              p.Id = projectId
            )
          ) |> ignore
        | Error e -> failwithf "Expected Ok but got Error: %A" e
      } |> TestHelpers.runAsync
    
    [<Fact>]
    let ``Service should handle null strings gracefully`` () =
      async {
        let mockRepo = Substitute.For<IProjectRepository>()
        let testService = ProjectService(mockRepo) :> IProjectService
        
        // Test null name
        let! result1 = testService.CreateProjectAsync(null, "Description", WebApp)
        TestHelpers.shouldBeError result1 |> TestHelpers.isValidationError |> should equal true
        
        // Test null description
        let! result2 = testService.CreateProjectAsync("Name", null, WebApp)
        TestHelpers.shouldBeError result2 |> TestHelpers.isValidationError |> should equal true
        
        // Verify no repository calls were made
        mockRepo.DidNotReceiveWithAnyArgs().CreateAsync(Unchecked.defaultof<Project>) |> ignore
      } |> TestHelpers.runAsync
  
  module BusinessRuleValidation =
    
    [<Fact>]
    let ``Multiple validation errors should return first error`` () =
      async {
        // Both name and description are invalid
        let! result = service.CreateProjectAsync("", "", WebApp)
        
        let error = TestHelpers.shouldBeError result
        match error with
        | ValidationFailed(field, _, _) ->
          // Should return name error first (based on validation order)
          field |> should equal "name"
        | _ -> failwith "Expected ValidationFailed error"
      } |> TestHelpers.runAsync
    
    [<Theory>]
    [<InlineData(99, true)>]
    [<InlineData(100, true)>]
    [<InlineData(101, false)>]
    let ``Name length validation boundary tests`` (length: int, shouldSucceed: bool) =
      async {
        let name = String.replicate length "a"
        let mockRepo = Substitute.For<IProjectRepository>()
        mockRepo.CreateAsync(Arg.Any<Project>())
          .Returns(async { return Ok () }) |> ignore
        
        let testService = ProjectService(mockRepo) :> IProjectService
        
        let! result = testService.CreateProjectAsync(name, "Description", WebApp)
        
        match shouldSucceed, result with
        | true, Ok _ -> ()
        | false, Error (ValidationFailed(field, reason, _)) ->
          field |> should equal "name"
          reason |> should haveSubstring "100 characters"
        | _ -> failwith $"Unexpected result for length {length}"
      } |> TestHelpers.runAsync
    
    [<Theory>]
    [<InlineData(999, true)>]
    [<InlineData(1000, true)>]
    [<InlineData(1001, false)>]
    let ``Description length validation boundary tests`` (length: int, shouldSucceed: bool) =
      async {
        let description = String.replicate length "a"
        let mockRepo = Substitute.For<IProjectRepository>()
        mockRepo.CreateAsync(Arg.Any<Project>())
          .Returns(async { return Ok () }) |> ignore
        
        let testService = ProjectService(mockRepo) :> IProjectService
        
        let! result = testService.CreateProjectAsync("Name", description, WebApp)
        
        match shouldSucceed, result with
        | true, Ok _ -> ()
        | false, Error (ValidationFailed(field, reason, _)) ->
          field |> should equal "description"
          reason |> should haveSubstring "1000 characters"
        | _ -> failwith $"Unexpected result for length {length}"
      } |> TestHelpers.runAsync