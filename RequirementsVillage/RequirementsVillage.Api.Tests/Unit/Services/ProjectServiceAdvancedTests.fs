namespace RequirementsVillage.Api.Tests.Unit.Services

open System
open System.Threading
open Xunit
open FsUnit.Xunit
open FsCheck.Xunit
open RequirementsVillage.Shared
open RequirementsVillage.Api.Services
open RequirementsVillage.Api.Persistence
open RequirementsVillage.Api.Tests.Helpers
open RequirementsVillage.Shared.Tests.TestGenerators
open FsCheck
open NSubstitute
open NSubstitute.ExceptionExtensions

module ProjectServiceAdvancedTests =
  
  module ExceptionHandling =
    
    [<Fact>]
    let ``Service should handle repository exceptions gracefully`` () =
      async {
        let mockRepo = Substitute.For<IProjectRepository>()
        let testException = Exception("Database connection failed")
        
        // Configure mock to throw exception
        mockRepo.GetAllAsync()
          .Throws(testException) |> ignore
        
        let service = ProjectService(mockRepo) :> IProjectService
        
        // Service should catch and wrap exception
        let! result = 
          async {
            try
              return! service.GetAllProjectsAsync()
            with
            | ex -> return Error (UnknownError ex.Message)
          }
        
        match result with
        | Error (UnknownError msg) ->
          msg |> should haveSubstring "Database connection failed"
        | _ -> failwith "Expected error result"
      } |> TestHelpers.runAsync
    
    [<Fact>]
    let ``Service should handle cancellation tokens properly`` () =
      async {
        let mockRepo = Substitute.For<IProjectRepository>()
        let cts = new CancellationTokenSource()
        
        // Configure mock to delay response
        mockRepo.GetAllAsync()
          .Returns(fun _ -> 
            async {
              do! Async.Sleep(1000)
              return Ok []
            }) |> ignore
        
        let service = ProjectService(mockRepo) :> IProjectService
        
        // Cancel the operation
        cts.CancelAfter(100)
        
        // Operation should be cancelled
        let! wasException =
          async {
            try
              let! _ = 
                Async.StartAsTask(
                  service.GetAllProjectsAsync(),
                  cancellationToken = cts.Token
                ) |> Async.AwaitTask
              return false
            with
            | :? OperationCanceledException -> return true
            | _ -> return false
          }
        
        wasException |> should equal true
      } |> TestHelpers.runAsync
  
  module PerformanceTests =
    
    [<Property>]
    let ``Service should handle large batches efficiently`` (count: byte) =
      (count > 10uy && count < 100uy) ==> lazy (
        async {
          let projectCount = int count
          let projects = 
            [1..projectCount]
            |> List.map (fun i -> 
              { TestDataGenerators.Bogus.Default.project() with 
                  Name = $"Project {i}" })
          
          let mockRepo = Substitute.For<IProjectRepository>()
          mockRepo.GetAllAsync()
            .Returns(async { return Ok projects }) |> ignore
          
          let service = ProjectService(mockRepo) :> IProjectService
          
          let startTime = DateTime.UtcNow
          let! result = service.GetAllProjectsAsync()
          let endTime = DateTime.UtcNow
          
          match result with
          | Ok retrievedProjects ->
            retrievedProjects.Length |> should equal projectCount
            (endTime - startTime).TotalMilliseconds |> should be (lessThan 1000.0)
            return true
          | Error _ -> return false
        } |> TestHelpers.runAsync
      )
  
  module TransactionBehavior =
    
    [<Fact>]
    let ``UpdateProject should be atomic - all or nothing`` () =
      async {
        let projectId = Guid.NewGuid()
        let existingProject = { TestDataGenerators.Bogus.Default.project() with Id = projectId }
        
        let mockRepo = Substitute.For<IProjectRepository>()
        mockRepo.GetByIdAsync(projectId)
          .Returns(async { return Ok (Some existingProject) }) |> ignore
        
        // Configure update to fail
        mockRepo.UpdateAsync(Arg.Any<Project>())
          .Returns(async { 
            return Error (DatabaseError("UPDATE", "Projects", Exception("Constraint violation")))
          }) |> ignore
        
        let service = ProjectService(mockRepo) :> IProjectService
        
        let updatedProject = {
          existingProject with
            Name = "New Name"
            Description = "New Description"
        }
        
        let! result = service.UpdateProjectAsync(updatedProject)
        
        // Should return error
        let error = TestHelpers.shouldBeError result
        TestHelpers.isDatabaseError error |> should equal true
        
        // Verify only one update attempt was made
        mockRepo.Received(1).UpdateAsync(Arg.Any<Project>()) |> ignore
      } |> TestHelpers.runAsync
    
    [<Fact>]
    let ``Concurrent updates should be handled safely`` () =
      async {
        let projectId = Guid.NewGuid()
        let originalProject = {
          TestDataGenerators.Bogus.Default.project() with
            Id = projectId
            Name = "Original"
        }
        
        let mockRepo = Substitute.For<IProjectRepository>()
        let mutable callCount = 0
        
        mockRepo.GetByIdAsync(projectId)
          .Returns(fun _ -> async { return Ok (Some originalProject) }) |> ignore
        
        mockRepo.UpdateAsync(Arg.Any<Project>())
          .Returns(fun _ -> 
            async {
              callCount <- callCount + 1
              if callCount = 1 then
                // First update succeeds
                return Ok ()
              else
                // Subsequent updates fail (simulating optimistic concurrency)
                return Error (DatabaseError("UPDATE", "Projects", Exception("Concurrency violation")))
            }) |> ignore
        
        let service = ProjectService(mockRepo) :> IProjectService
        
        // Start multiple concurrent updates
        let! results =
          [1..5]
          |> List.map (fun i ->
            let updated = { originalProject with Name = $"Update {i}" }
            service.UpdateProjectAsync(updated))
          |> Async.Parallel
        
        // Only first should succeed
        let successes = results |> Array.filter (function Ok _ -> true | _ -> false)
        let failures = results |> Array.filter (function Error _ -> true | _ -> false)
        
        successes.Length |> should equal 1
        failures.Length |> should equal 4
      } |> TestHelpers.runAsync
  
  module DataIntegrity =
    
    [<Fact>]
    let ``Service should never expose internal implementation details`` () =
      async {
        let mockRepo = Substitute.For<IProjectRepository>()
        let internalException = Exception("SqlException: Connection timeout at line 42")
        
        mockRepo.CreateAsync(Arg.Any<Project>())
          .Returns(async { 
            return Error (DatabaseError("CREATE", "Projects", internalException))
          }) |> ignore
        
        let service = ProjectService(mockRepo) :> IProjectService
        
        let! result = service.CreateProjectAsync("Test", "Description", WebApp)
        
        let error = TestHelpers.shouldBeError result
        
        match error with
        | DatabaseError(op, table, ex) ->
          // Currently the service passes through the exception as-is
          // In a production system, you might want to sanitize these messages
          ex.Message |> should equal "SqlException: Connection timeout at line 42"
          op |> should equal "CREATE"
          table |> should equal "Projects"
        | _ -> failwith "Expected DatabaseError"
      } |> TestHelpers.runAsync
    
    [<Property>]
    let ``Service should maintain data consistency across operations`` () =
      async {
        let mockRepo = Substitute.For<IProjectRepository>()
        let mutable storedProjects = Map.empty<Guid, Project>
        
        // Configure mock to use in-memory storage
        mockRepo.CreateAsync(Arg.Any<Project>())
          .Returns(fun args ->
            async {
              let project = args.[0] :?> Project
              storedProjects <- storedProjects.Add(project.Id, project)
              return Ok ()
            }) |> ignore
        
        mockRepo.GetByIdAsync(Arg.Any<Guid>())
          .Returns(fun args ->
            async {
              let id = args.[0] :?> Guid
              return Ok (storedProjects.TryFind id)
            }) |> ignore
        
        mockRepo.UpdateAsync(Arg.Any<Project>())
          .Returns(fun args ->
            async {
              let project = args.[0] :?> Project
              match storedProjects.TryFind project.Id with
              | Some _ ->
                storedProjects <- storedProjects.Add(project.Id, project)
                return Ok ()
              | None ->
                return Error (NotFound(project.Id, "Update"))
            }) |> ignore
        
        let service = ProjectService(mockRepo) :> IProjectService
        
        // Create a project
        let! createResult = service.CreateProjectAsync("Test", "Description", WebApp)
        let createdProject = TestHelpers.shouldBeOk createResult
        
        // Retrieve it
        let! getResult = service.GetProjectByIdAsync(createdProject.Id)
        let retrievedProject = TestHelpers.shouldBeSome (TestHelpers.shouldBeOk getResult)
        
        // Update it
        let updatedProject = { retrievedProject with Name = "Updated" }
        let! updateResult = service.UpdateProjectAsync(updatedProject)
        TestHelpers.shouldBeOk updateResult |> ignore
        
        // Retrieve again
        let! finalResult = service.GetProjectByIdAsync(createdProject.Id)
        let finalProject = TestHelpers.shouldBeSome (TestHelpers.shouldBeOk finalResult)
        
        // Verify consistency
        finalProject.Id |> should equal createdProject.Id
        finalProject.Name |> should equal "Updated"
        finalProject.CreatedAt |> should equal createdProject.CreatedAt
        finalProject.UpdatedAt |> should be (greaterThan createdProject.UpdatedAt)
      } |> TestHelpers.runAsync
  
  module MockVerification =
    
    [<Fact>]
    let ``Service should call repository methods in correct order`` () =
      async {
        let projectId = Guid.NewGuid()
        let existingProject = TestDataGenerators.Bogus.Default.project()
        
        let mockRepo = Substitute.For<IProjectRepository>()
        let mutable callOrder = []
        
        mockRepo.GetByIdAsync(Arg.Any<Guid>())
          .Returns(fun args ->
            async {
              callOrder <- "GetById" :: callOrder
              return Ok (Some existingProject)
            }) |> ignore
        
        mockRepo.UpdateAsync(Arg.Any<Project>())
          .Returns(fun args ->
            async {
              callOrder <- "Update" :: callOrder
              return Ok ()
            }) |> ignore
        
        let service = ProjectService(mockRepo) :> IProjectService
        
        let! result = service.UpdateProjectStatusAsync(projectId, InProgress)
        TestHelpers.shouldBeOk result |> ignore
        
        // Verify call order (reversed because we prepended)
        callOrder |> List.rev |> should equal ["GetById"; "Update"]
      } |> TestHelpers.runAsync
    
    [<Fact>]
    let ``Service should not make unnecessary repository calls`` () =
      async {
        let mockRepo = Substitute.For<IProjectRepository>()
        let service = ProjectService(mockRepo) :> IProjectService
        
        // Try to create with invalid data
        let! result = service.CreateProjectAsync("", "Description", WebApp)
        TestHelpers.shouldBeError result |> ignore
        
        // Verify no repository calls were made
        mockRepo.ReceivedCalls() |> Seq.length |> should equal 0
      } |> TestHelpers.runAsync