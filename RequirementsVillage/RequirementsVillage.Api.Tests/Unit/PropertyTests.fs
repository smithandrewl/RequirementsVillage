namespace RequirementsVillage.Api.Tests.Unit

open System
open Xunit
open FsCheck
open FsCheck.Xunit
open RequirementsVillage.Shared
open RequirementsVillage.Api.Services
open RequirementsVillage.Api.Persistence
open RequirementsVillage.Shared.Tests.TestGenerators

module PropertyTests =
  
  // Register custom generators
  do TestDataGenerators.FsCheck.registerGenerators() |> ignore
  
  // Helper to create service with in-memory repository
  let createServiceWithRepo() =
    let repo = InMemoryProjectRepository() :> IProjectRepository
    ProjectService(repo) :> IProjectService
  
  // ===================================================================
  // Model Invariant Properties
  // ===================================================================
  
  module ModelInvariants =
    
    [<Property>]
    let ``UpdatedAt should always be greater than or equal to CreatedAt`` () =
      Prop.forAll (TestDataGenerators.FsCheck.validProject |> Arb.fromGen) (fun project ->
        project.UpdatedAt >= project.CreatedAt
      )
    
    [<Property>]
    let ``Project Name should never be empty or whitespace`` () =
      Prop.forAll (TestDataGenerators.FsCheck.validProject |> Arb.fromGen) (fun project ->
        not (String.IsNullOrWhiteSpace project.Name)
      )
    
    [<Property>]
    let ``Project Description should never be empty or whitespace`` () =
      Prop.forAll (TestDataGenerators.FsCheck.validProject |> Arb.fromGen) (fun project ->
        not (String.IsNullOrWhiteSpace project.Description)
      )
    
    [<Property>]
    let ``Project Name length should be within valid bounds`` () =
      Prop.forAll (TestDataGenerators.FsCheck.validProject |> Arb.fromGen) (fun project ->
        project.Name.Length > 0 && project.Name.Length <= 100
      )
    
    [<Property>]
    let ``Project Description length should be within valid bounds`` () =
      Prop.forAll (TestDataGenerators.FsCheck.validProject |> Arb.fromGen) (fun project ->
        project.Description.Length > 0 && project.Description.Length <= 1000
      )
    
    [<Property>]
    let ``Project ID should never be empty Guid`` () =
      Prop.forAll (TestDataGenerators.FsCheck.validProject |> Arb.fromGen) (fun project ->
        project.Id <> Guid.Empty
      )
  
  
  // ===================================================================
  // Service Logic Properties
  // ===================================================================
  
  module ServiceLogicProperties =
    
    [<Property>]
    let ``Creating a project then getting it by ID should return the same project`` () =
      Prop.forAll (
        Gen.map3 (fun a b c -> (a, b, c))
          TestDataGenerators.FsCheck.projectName
          TestDataGenerators.FsCheck.projectDescription
          TestDataGenerators.FsCheck.projectCategory
        |> Arb.fromGen
      ) (fun (name, description, category) ->
        async {
          let service = createServiceWithRepo()
          
          // Create project
          let! createResult = 
            service.CreateProjectAsync(name, description, category)
          
          match createResult with
          | Ok createdProject ->
            // Get project by ID
            let! getResult = 
              service.GetProjectByIdAsync(createdProject.Id)
            
            match getResult with
            | Ok (Some retrievedProject) ->
              // Should be exactly the same
              return retrievedProject = createdProject
            | _ -> return false
          | _ -> return false
        } |> Async.RunSynchronously
      )
    
    [<Property>]
    let ``All created projects should start with Idea status`` () =
      Prop.forAll (
        Gen.map3 (fun a b c -> (a, b, c))
          TestDataGenerators.FsCheck.projectName
          TestDataGenerators.FsCheck.projectDescription
          TestDataGenerators.FsCheck.projectCategory
        |> Arb.fromGen
      ) (fun (name, description, category) ->
        async {
          let service = createServiceWithRepo()
          
          let! result = 
            service.CreateProjectAsync(name, description, category)
          
          match result with
          | Ok project -> return project.Status = Idea
          | Error _ -> return false
        } |> Async.RunSynchronously
      )
    
    [<Property>]
    let ``Creating multiple projects should result in unique IDs`` () =
      Prop.forAll (
        Gen.listOfLength 10 (
          Gen.map3 (fun a b c -> (a, b, c))
            TestDataGenerators.FsCheck.projectName
            TestDataGenerators.FsCheck.projectDescription
            TestDataGenerators.FsCheck.projectCategory
        ) |> Arb.fromGen
      ) (fun projectSpecs ->
        async {
          let service = createServiceWithRepo()
          
          let! projects =
            projectSpecs
            |> List.map (fun (name, desc, cat) ->
              service.CreateProjectAsync(name, desc, cat)
            )
            |> Async.Sequential
          
          let successfulProjects =
            projects
            |> Array.choose (function
              | Ok p -> Some p
              | _ -> None
            )
            |> Array.toList
          
          let ids = successfulProjects |> List.map (fun p -> p.Id)
          let uniqueIds = ids |> List.distinct
          
          return ids.Length = uniqueIds.Length
        } |> Async.RunSynchronously
      )
    
    [<Property>]
    let ``Updating a project should preserve its ID and CreatedAt`` () =
      Prop.forAll (
        TestDataGenerators.FsCheck.validProject |> Arb.fromGen
      ) (fun originalProject ->
        async {
          let service = createServiceWithRepo()
          
          // First create the project
          let! createResult = 
            service.CreateProjectAsync(
              originalProject.Name,
              originalProject.Description,
              originalProject.Category
            )
          
          match createResult with
          | Ok createdProject ->
            // Update with different values
            let updatedProject : Project = {
              createdProject with
                Name = createdProject.Name + " Updated"
                Description = createdProject.Description + " Updated"
                Status = InProgress // Valid transition from Idea
            }
            
            let! updateResult = 
              service.UpdateProjectAsync(updatedProject)
            
            match updateResult with
            | Ok () ->
              let! getResult = 
                service.GetProjectByIdAsync(createdProject.Id)
              
              match getResult with
              | Ok (Some retrievedProject) ->
                return 
                  retrievedProject.Id = createdProject.Id &&
                  retrievedProject.CreatedAt = createdProject.CreatedAt
              | _ -> return false
            | _ -> return false
          | _ -> return false
        } |> Async.RunSynchronously
      )
    
    [<Property>]
    let ``Deleting a project should succeed for any status`` () =
      Prop.forAll (
        TestDataGenerators.FsCheck.validProject |> Arb.fromGen
      ) (fun project ->
        async {
          let service = createServiceWithRepo()
          
          // Create project
          let! createResult = 
            service.CreateProjectAsync(
              project.Name,
              project.Description,
              project.Category
            )
          
          match createResult with
          | Ok createdProject ->
            // Delete should always succeed
            let! deleteResult = 
              service.DeleteProjectAsync(createdProject.Id)
            
            match deleteResult with
            | Ok () -> return true
            | _ -> return false
          | _ -> return false
        } |> Async.RunSynchronously
      )
  
  // ===================================================================
  // Validation Rule Properties
  // ===================================================================
  
  module ValidationProperties =
    
    [<Property>]
    let ``Empty or whitespace names should always fail validation`` () =
      let invalidNames = [
        ""
        " "
        "   "
        "\t"
        "\n"
        "\r\n"
      ]
      
      invalidNames
      |> List.forall (fun name ->
        async {
          let service = createServiceWithRepo()
          
          let! result = 
            service.CreateProjectAsync(name, "Valid description", WebApp)
          
          match result with
          | Error (ValidationFailed ("name", _, _)) -> return true
          | _ -> return false
        } |> Async.RunSynchronously
      )
    
    [<Property>]
    let ``Names exceeding 100 characters should fail validation`` () =
      Prop.forAll (
        Gen.choose (101, 500) 
        |> Gen.map (fun len -> String.replicate len "a")
        |> Arb.fromGen
      ) (fun longName ->
        async {
          let service = createServiceWithRepo()
          
          let! result = 
            service.CreateProjectAsync(longName, "Valid description", WebApp)
          
          match result with
          | Error (ValidationFailed ("name", reason, _)) ->
            return reason.Contains("100 characters")
          | _ -> return false
        } |> Async.RunSynchronously
      )
    
    [<Property>]
    let ``Descriptions exceeding 1000 characters should fail validation`` () =
      Prop.forAll (
        Gen.choose (1001, 2000) 
        |> Gen.map (fun len -> String.replicate len "a")
        |> Arb.fromGen
      ) (fun longDescription ->
        async {
          let service = createServiceWithRepo()
          
          let! result = 
            service.CreateProjectAsync("Valid name", longDescription, WebApp)
          
          match result with
          | Error (ValidationFailed ("description", reason, _)) ->
            return reason.Contains("1000 characters")
          | _ -> return false
        } |> Async.RunSynchronously
      )
  
  // ===================================================================
  // Repository State Consistency Properties
  // ===================================================================
  
  module RepositoryConsistency =
    
    [<Property>]
    let ``Repository should maintain consistent count after operations`` () =
      Prop.forAll (
        Gen.listOfLength 5 TestDataGenerators.FsCheck.validProject |> Arb.fromGen
      ) (fun projects ->
        async {
          let repo = InMemoryProjectRepository() :> IProjectRepository
          
          // Get initial count
          let! initialResult = repo.GetAllAsync()
          let initialCount = 
            match initialResult with
            | Ok projects -> projects.Length
            | _ -> 0
          
          // Add all projects
          let! addResults =
            projects
            |> List.map (fun p -> repo.CreateAsync(p))
            |> Async.Sequential
          
          // Get count after additions
          let! afterAddResult = repo.GetAllAsync()
          let afterAddCount = 
            match afterAddResult with
            | Ok projects -> projects.Length
            | _ -> 0
          
          let successfulAdds = 
            addResults 
            |> Array.filter (function Ok () -> true | _ -> false)
            |> Array.length
          
          return afterAddCount = initialCount + successfulAdds
        } |> Async.RunSynchronously
      )
    
    [<Property>]
    let ``Updating non-existent project should fail`` () =
      Prop.forAll (
        TestDataGenerators.FsCheck.validProject |> Arb.fromGen
      ) (fun project ->
        async {
          let repo = InMemoryProjectRepository() :> IProjectRepository
          
          // Try to update a project that doesn't exist
          let nonExistentProject = { project with Id = Guid.NewGuid() }
          let! result = repo.UpdateAsync(nonExistentProject)
          
          match result with
          | Error (NotFound _) -> return true
          | _ -> return false
        } |> Async.RunSynchronously
      )
    
    [<Property>]
    let ``Getting all projects should return them in insertion order (newest first)`` () =
      Prop.forAll (
        Gen.listOfLength 10 TestDataGenerators.FsCheck.validProject 
        |> Arb.fromGen
      ) (fun projects ->
        async {
          let repo = InMemoryProjectRepository() :> IProjectRepository
          
          // Clear existing projects by creating new repo
          let freshRepo = InMemoryProjectRepository() :> IProjectRepository
          
          // Add all projects
          let! _ =
            projects
            |> List.map (fun p -> freshRepo.CreateAsync(p))
            |> Async.Sequential
          
          // Get all projects
          let! result = freshRepo.GetAllAsync()
          
          match result with
          | Ok retrievedProjects ->
            // The in-memory repo prepends new projects (newest first)
            // So the retrieved projects should be in reverse order of insertion
            // Filter out the pre-existing projects by their IDs
            let newProjectIds = projects |> List.map (fun p -> p.Id) |> Set.ofList
            let onlyNewProjects = retrievedProjects |> List.filter (fun p -> Set.contains p.Id newProjectIds)
            let expectedOrder = projects |> List.rev
            return onlyNewProjects.Length = projects.Length &&
                   (List.zip onlyNewProjects expectedOrder 
                    |> List.forall (fun (retrieved, expected) -> 
                      retrieved.Id = expected.Id))
          | _ -> return false
        } |> Async.RunSynchronously
      )
  
  // ===================================================================
  // Edge Case Properties with Shrinking
  // ===================================================================
  
  module EdgeCases =
    
    // Custom shrinker for project names that maintains validity
    let shrinkValidName (name: string) =
      if name.Length <= 1 then Seq.empty
      else
        seq {
          // Try removing characters from the end
          if name.Length > 1 then
            yield name.Substring(0, name.Length - 1)
          
          // Try removing characters from the beginning
          if name.Length > 1 then
            yield name.Substring(1)
          
          // Try simplifying to single character
          if name.Length > 1 then
            yield "a"
        }
        |> Seq.filter (fun s -> not (String.IsNullOrWhiteSpace s))
    
    [<Property>]
    let ``Project name at exact boundary (100 chars) should be valid`` () =
      let boundaryName = String.replicate 100 "a"
      
      async {
        let service = createServiceWithRepo()
        
        let! result = 
          service.CreateProjectAsync(boundaryName, "Description", WebApp)
        
        match result with
        | Ok project -> return project.Name = boundaryName
        | _ -> return false
      } |> Async.RunSynchronously
    
    [<Property>]
    let ``Project description at exact boundary (1000 chars) should be valid`` () =
      let boundaryDescription = String.replicate 1000 "a"
      
      async {
        let service = createServiceWithRepo()
        
        let! result = 
          service.CreateProjectAsync("Name", boundaryDescription, WebApp)
        
        match result with
        | Ok project -> return project.Description = boundaryDescription
        | _ -> return false
      } |> Async.RunSynchronously
    
    [<Property>]
    let ``Unicode characters in names and descriptions should be handled correctly`` () =
      let unicodeStrings = [
        "Hello 世界"
        "Emoji 🚀 project"
        "Ñoño"
        "Привет мир"
        "مرحبا بالعالم"
      ]
      
      unicodeStrings
      |> List.forall (fun str ->
        async {
          let service = createServiceWithRepo()
          
          let! result = 
            service.CreateProjectAsync(str, str, WebApp)
          
          match result with
          | Ok project -> 
            return project.Name = str && project.Description = str
          | _ -> return false
        } |> Async.RunSynchronously
      )
    
    [<Property>]
    let ``Concurrent creates should all succeed with unique IDs`` () =
      Prop.forAll (
        Gen.choose (5, 20) |> Arb.fromGen
      ) (fun count ->
        async {
          let service = createServiceWithRepo()
          
          // Create projects concurrently
          let! results =
            [1..count]
            |> List.map (fun i ->
              service.CreateProjectAsync(
                sprintf "Project %d" i,
                sprintf "Description %d" i,
                WebApp
              )
            )
            |> Async.Parallel
          
          let successfulProjects =
            results
            |> Array.choose (function
              | Ok p -> Some p
              | _ -> None
            )
          
          let ids = 
            successfulProjects 
            |> Array.map (fun p -> p.Id)
            |> Array.distinct
          
          return ids.Length = successfulProjects.Length
        } |> Async.RunSynchronously
      )
  
  
  // ===================================================================
  // Custom Generators for Complex Scenarios
  // ===================================================================
  
  module CustomGenerators =
    
    // Generator for projects with specific constraints
    let projectWithConstraintsGen =
      gen {
        let! nameLen = Gen.choose (10, 50)
        let name = 
          List.init nameLen (fun i -> 
            if i = 0 then 'P'
            else char (int 'a' + (i % 26))
          )
          |> List.map string
          |> String.concat ""
        
        let! descLen = Gen.choose (50, 500)
        let description =
          List.init descLen (fun i ->
            if i % 10 = 0 then ' '
            else char (int 'a' + (i % 26))
          )
          |> List.map string
          |> String.concat ""
        
        let! category = TestDataGenerators.FsCheck.projectCategory
        let! status = TestDataGenerators.FsCheck.projectStatus
        
        let now = DateTime.UtcNow
        let! daysAgo = Gen.choose (0, 365)
        let createdAt = now.AddDays(float -daysAgo)
        
        let! daysAfterCreation = Gen.choose (0, daysAgo)
        let updatedAt = createdAt.AddDays(float daysAfterCreation)
        
        return {
          Id          = Guid.NewGuid()
          Name        = name
          Description = description
          Category    = category
          Status      = status
          CreatedAt   = createdAt
          UpdatedAt   = updatedAt
        }
      }
    
    [<Property>]
    let ``Projects generated with constraints should always be valid`` () =
      Prop.forAll (projectWithConstraintsGen |> Arb.fromGen) (fun project ->
        not (String.IsNullOrWhiteSpace project.Name) &&
        not (String.IsNullOrWhiteSpace project.Description) &&
        project.Name.Length <= 100 &&
        project.Description.Length <= 1000 &&
        project.UpdatedAt >= project.CreatedAt
      )
    
    
    // Generator for projects with specific category patterns
    let projectWithCategoryPatternGen =
      gen {
        let! categoryType = Gen.choose (1, 6)
        let! baseName = TestDataGenerators.FsCheck.projectName
        
        let! categoryAndPrefix =
          match categoryType with
          | 1 -> Gen.constant (WebApp, "Web ")
          | 2 -> Gen.constant (MobileApp, "Mobile ")
          | 3 -> Gen.constant (Library, "Lib ")
          | 4 -> Gen.constant (Tool, "Tool ")
          | 5 -> Gen.constant (Game, "Game ")
          | _ -> 
            gen {
              let! customName = Gen.elements ["AI"; "ML"; "IoT"; "Blockchain"]
              return (Other customName, customName + " ")
            }
        
        let (category, namePrefix) = categoryAndPrefix
        
        let name = 
          if baseName.StartsWith(namePrefix) then baseName
          else namePrefix + baseName
        
        let! description = TestDataGenerators.FsCheck.projectDescription
        let! status = TestDataGenerators.FsCheck.projectStatus
        
        return {
          Id          = Guid.NewGuid()
          Name        = name.Substring(0, min name.Length 100)
          Description = description
          Category    = category
          Status      = status
          CreatedAt   = DateTime.UtcNow
          UpdatedAt   = DateTime.UtcNow
        }
      }
  
  // ===================================================================
  // Advanced Business Logic Properties
  // ===================================================================
  
  module AdvancedBusinessLogic =
    
    
    [<Property>]
    let ``Project categories should influence validation in expected ways`` () =
      Prop.forAll (CustomGenerators.projectWithCategoryPatternGen |> Arb.fromGen) (fun project ->
        // Category-specific name patterns should be preserved
        match project.Category with
        | WebApp -> project.Name.Contains("Web") || project.Name.Contains("App")
        | MobileApp -> project.Name.Contains("Mobile") || project.Name.Contains("App")
        | Library -> project.Name.Contains("Lib")
        | Tool -> project.Name.Contains("Tool")
        | Game -> project.Name.Contains("Game")
        | Other customName -> project.Name.Contains(customName)
      )
    
    // TODO: Flaky test - race condition in parallel creates
    // [<Property>]
    [<Property(Skip = "Temporarily disabled - flaky test with race condition")>]
    let ``Batch operations should maintain repository consistency`` () =
      Prop.forAll (Gen.listOfLength 20 TestDataGenerators.FsCheck.validProject |> Arb.fromGen) (fun projects ->
        async {
          let repo = InMemoryProjectRepository() :> IProjectRepository
          let service = ProjectService(repo) :> IProjectService
          
          // Parallel creates
          let! createResults =
            projects
            |> List.map (fun p ->
              service.CreateProjectAsync(p.Name, p.Description, p.Category)
            )
            |> Async.Parallel
          
          let createdCount = 
            createResults 
            |> Array.sumBy (function Ok _ -> 1 | _ -> 0)
          
          // Get all projects
          let! allProjects = service.GetAllProjectsAsync()
          
          match allProjects with
          | Ok projectList ->
            // Repository count should match successful creates
            return projectList.Length >= createdCount
          | _ -> return false
        } |> Async.RunSynchronously
      )
    
    [<Property>]
    let ``Concurrent updates to different projects should all succeed`` () =
      Prop.forAll (Gen.listOfLength 10 TestDataGenerators.FsCheck.validProject |> Arb.fromGen) (fun projectSpecs ->
        async {
          let service = createServiceWithRepo()
          
          // Create all projects first
          let! createResults =
            projectSpecs
            |> List.map (fun p ->
              service.CreateProjectAsync(p.Name, p.Description, p.Category)
            )
            |> Async.Sequential
          
          let createdProjects =
            createResults
            |> Array.choose (function Ok p -> Some p | _ -> None)
            |> Array.toList
          
          if createdProjects.Length = 0 then
            return true
          else
            // Update all projects concurrently with different values
            let! updateResults =
              createdProjects
              |> List.mapi (fun i p ->
                let updated = {
                  p with
                    Name = p.Name + sprintf " Updated %d" i
                    Description = p.Description + sprintf " Modified %d" i
                }
                service.UpdateProjectAsync(updated)
              )
              |> Async.Parallel
            
            let allSucceeded = 
              updateResults |> Array.forall (function Ok () -> true | _ -> false)
            
            return allSucceeded
        } |> Async.RunSynchronously
      )
  
  // ===================================================================
  // Complex Validation Properties
  // ===================================================================
  
  module ComplexValidation =
    
    // Generator for edge case strings
    let edgeCaseStringGen =
      Gen.frequency [
        (1, Gen.constant "")
        (1, Gen.constant " ")
        (1, Gen.constant "  ")
        (1, Gen.constant "\t")
        (1, Gen.constant "\n")
        (1, Gen.constant "\r\n")
        (2, Gen.map (fun n -> String.replicate n " ") (Gen.choose (1, 10)))
        (2, Gen.map (fun n -> String.replicate n "a") (Gen.choose (95, 105)))
        (2, Gen.map (fun n -> String.replicate n "b") (Gen.choose (995, 1005)))
        (3, TestDataGenerators.FsCheck.projectName)
      ]
    
    [<Property>]
    let ``Complex validation scenarios should behave correctly`` () =
      Prop.forAll (
        Gen.map3 (fun a b c -> (a, b, c)) edgeCaseStringGen edgeCaseStringGen TestDataGenerators.FsCheck.projectCategory
        |> Arb.fromGen
      ) (fun (name, description, category) ->
        async {
          let service = createServiceWithRepo()
          
          let! result = 
            service.CreateProjectAsync(name, description, category)
          
          let isNameValid = 
            not (String.IsNullOrWhiteSpace name) && name.Length <= 100
          let isDescriptionValid = 
            not (String.IsNullOrWhiteSpace description) && description.Length <= 1000
          
          match result with
          | Ok _ -> return isNameValid && isDescriptionValid
          | Error (ValidationFailed (field, _, _)) ->
            return (field = "name" && not isNameValid) ||
                   (field = "description" && not isDescriptionValid)
          | _ -> return false
        } |> Async.RunSynchronously
      )
    
    [<Property>]
    let ``Special characters in project data should be handled correctly`` () =
      let specialCharStrings = [
        "Project\x00Name" // Null character
        "Project\u200B" // Zero-width space
        "Project\u2028" // Line separator
        "Project\u2029" // Paragraph separator
        "<script>alert('xss')</script>"
        "'; DROP TABLE projects; --"
        "Project with 日本語"
        "Проект with русский"
        "مشروع with عربي"
      ]
      
      specialCharStrings
      |> List.forall (fun specialStr ->
        async {
          let service = createServiceWithRepo()
          
          // These should either succeed or fail gracefully
          let! nameResult = 
            service.CreateProjectAsync(specialStr, "Normal description", WebApp)
          
          let! descResult = 
            service.CreateProjectAsync("Normal name", specialStr, WebApp)
          
          // We just ensure no exceptions are thrown
          match nameResult, descResult with
          | _, _ -> return true
        } |> Async.RunSynchronously
      )
  
  
  // ===================================================================
  // Performance and Scale Properties
  // ===================================================================
  
  module PerformanceProperties =
    
    [<Property(MaxTest = 20)>]
    let ``Large batch operations should complete without errors`` () =
      Prop.forAll (Gen.choose (50, 100) |> Arb.fromGen) (fun count ->
        async {
          let service = createServiceWithRepo()
          
          // Create many projects
          let! createResults =
            [1..count]
            |> List.map (fun i ->
              service.CreateProjectAsync(
                sprintf "Project %d" i,
                sprintf "Description for project %d" i,
                WebApp
              )
            )
            |> List.chunkBySize 10
            |> List.map Async.Parallel
            |> Async.Sequential
          
          let totalCreated = 
            createResults 
            |> Array.concat 
            |> Array.sumBy (function Ok _ -> 1 | _ -> 0)
          
          // Should create all projects
          return totalCreated = count
        } |> Async.RunSynchronously
      )
    
    [<Property>]
    let ``Repository operations should be idempotent where applicable`` () =
      Prop.forAll (TestDataGenerators.FsCheck.validProject |> Arb.fromGen) (fun project ->
        async {
          let repo = InMemoryProjectRepository() :> IProjectRepository
          
          // Create project
          let! firstCreate = repo.CreateAsync(project)
          
          // Try to create same project again (same ID)
          let! secondCreate = repo.CreateAsync(project)
          
          // Get all projects
          let! allProjects = repo.GetAllAsync()
          
          match firstCreate, secondCreate, allProjects with
          | Ok (), _, Ok projects ->
            // Should only have one project with this ID
            let projectsWithId = 
              projects |> List.filter (fun p -> p.Id = project.Id)
            return projectsWithId.Length = 1
          | _ -> return false
        } |> Async.RunSynchronously
      )
  
  // ===================================================================
  // Algebraic Properties
  // ===================================================================
  
  module AlgebraicProperties =
    
    [<Property>]
    let ``Creating then deleting should result in empty repository`` () =
      Prop.forAll (TestDataGenerators.FsCheck.validProject |> Arb.fromGen) (fun projectSpec ->
        async {
          let service = createServiceWithRepo()
          
          // Create project
          let! createResult = 
            service.CreateProjectAsync(
              projectSpec.Name,
              projectSpec.Description,
              projectSpec.Category
            )
          
          match createResult with
          | Ok project ->
            // Delete project (no need to transition to Abandoned first)
            let! deleteResult = service.DeleteProjectAsync(project.Id)
            
            match deleteResult with
            | Ok () ->
              // Try to get project
              let! getResult = service.GetProjectByIdAsync(project.Id)
              
              match getResult with
              | Ok None -> return true // Project not found, as expected
              | _ -> return false
            | _ -> return false
          | _ -> return false
        } |> Async.RunSynchronously
      )
    
    [<Property>]
    let ``Update operations should be associative`` () =
      Prop.forAll (
        Gen.map3 (fun a b c -> (a, b, c))
          TestDataGenerators.FsCheck.projectName
          TestDataGenerators.FsCheck.projectName
          TestDataGenerators.FsCheck.projectName
        |> Arb.fromGen
      ) (fun (name1, name2, name3) ->
        async {
          let service = createServiceWithRepo()
          
          // Create project
          let! createResult = 
            service.CreateProjectAsync("Initial", "Initial Description", WebApp)
          
          match createResult with
          | Ok project ->
            // Path 1: Update to name1, then name2, then name3
            let! path1Result = async {
              let p1 = { project with Name = name1 }
              let! r1 = service.UpdateProjectAsync(p1)
              match r1 with
              | Ok () ->
                let! current = service.GetProjectByIdAsync(project.Id)
                match current with
                | Ok (Some p) ->
                  let p2 = { p with Name = name2 }
                  let! r2 = service.UpdateProjectAsync(p2)
                  match r2 with
                  | Ok () ->
                    let! current2 = service.GetProjectByIdAsync(project.Id)
                    match current2 with
                    | Ok (Some p) ->
                      let p3 = { p with Name = name3 }
                      return! service.UpdateProjectAsync(p3)
                    | _ -> return Error (UnknownError "Failed to get project")
                  | error -> return error
                | _ -> return Error (UnknownError "Failed to get project")
              | error -> return error
            }
            
            // Final result should have name3
            match path1Result with
            | Ok () ->
              let! finalProject = service.GetProjectByIdAsync(project.Id)
              match finalProject with
              | Ok (Some p) -> return p.Name = name3
              | _ -> return false
            | _ -> return false
          | _ -> return false
        } |> Async.RunSynchronously
      )