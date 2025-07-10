namespace RequirementsVillage.Shared.TestGenerators

open System
open Bogus
open Bogus.DataSets
open FsCheck
open RequirementsVillage.Shared

/// Shared test data generators for use across all test projects
module TestDataGenerators =
  
  // Initialize Bogus with deterministic seed for reproducible tests
  let createFaker locale = Faker(locale)
  let defaultFaker = 
    let faker = createFaker "en"
    faker.Random <- Randomizer(12345) // Set seed for deterministic results
    faker
  
  /// Bogus-based generators for realistic test data
  module Bogus =
    
    let projectName (faker: Faker) =
      faker.Random.ArrayElement([|
        faker.Commerce.ProductName()
        faker.Company.CatchPhrase()
        sprintf "%s %s" (faker.Hacker.Noun()) (faker.Hacker.Noun())
        sprintf "%s Manager" (faker.Lorem.Word())
        sprintf "%s Tracker" (faker.Lorem.Word())
        faker.Lorem.Sentence(3).TrimEnd('.')
      |])
    
    let projectDescription (faker: Faker) =
      faker.Lorem.Paragraph()
    
    let projectCategory (faker: Faker) =
      faker.Random.ArrayElement([|
        WebApp
        MobileApp
        Library
        Tool
        Game
        Other (faker.Lorem.Word())
      |])
    
    let projectStatus (faker: Faker) =
      faker.Random.ArrayElement([|
        Idea
        InProgress
        Completed
        Abandoned
        OnHold
      |])
    
    let project (faker: Faker) =
      let createdAt = faker.Date.Past(2, DateTime.UtcNow)
      let updatedAt = faker.Date.Between(createdAt, DateTime.UtcNow)
      {
        Id          = Guid.NewGuid()
        Name        = projectName faker
        Description = projectDescription faker
        Category    = projectCategory faker
        Status      = projectStatus faker
        CreatedAt   = createdAt
        UpdatedAt   = updatedAt
      }
    
    let projectWithStatus (faker: Faker) (status: ProjectStatus) =
      { project faker with Status = status }
    
    let projectWithCategory (faker: Faker) (category: ProjectCategory) =
      { project faker with Category = category }
    
    let projects (faker: Faker) (count: int) =
      [ for _ in 1..count -> project faker ]
    
    let projectsWithMixedStatuses (faker: Faker) =
      [
        projectWithStatus faker Idea
        projectWithStatus faker InProgress
        projectWithStatus faker Completed
        projectWithStatus faker Abandoned
        projectWithStatus faker OnHold
      ]
    
    // Default generators using the seeded faker
    module Default =
      let projectName() = projectName defaultFaker
      let projectDescription() = projectDescription defaultFaker
      let projectCategory() = projectCategory defaultFaker
      let projectStatus() = projectStatus defaultFaker
      let project() = project defaultFaker
      let projectWithStatus status = projectWithStatus defaultFaker status
      let projectWithCategory category = projectWithCategory defaultFaker category
      let projects count = projects defaultFaker count
      let projectsWithMixedStatuses() = projectsWithMixedStatuses defaultFaker
  
  /// FsCheck generators for property-based testing
  module FsCheck =
    
    let nonEmptyString =
      Arb.generate<NonEmptyString>
      |> Gen.map (fun (NonEmptyString s) -> s)
    
    let projectName =
      Gen.sized (fun size ->
        nonEmptyString
        |> Gen.filter (fun s -> s.Length > 0 && s.Length <= 100)
      )
    
    let projectDescription =
      Gen.sized (fun size ->
        nonEmptyString
        |> Gen.filter (fun s -> s.Length > 0 && s.Length <= 1000)
      )
    
    let projectStatus =
      Gen.elements [
        Idea
        InProgress
        Completed
        Abandoned
        OnHold
      ]
    
    let projectCategory =
      Gen.frequency [
        (5, Gen.elements [ WebApp; MobileApp; Library; Tool; Game ])
        (1, Gen.map Other (nonEmptyString |> Gen.filter (fun s -> s <> null)))
      ]
    
    let validProject =
      gen {
        let! name        = projectName
        let! description = projectDescription
        let! category    = projectCategory
        let! status      = projectStatus
        let! created     = Arb.generate<DateTime>
        let! updated     = Arb.generate<DateTime>
        
        return {
          Id          = Guid.NewGuid()
          Name        = name
          Description = description
          Category    = category
          Status      = status
          CreatedAt   = created
          UpdatedAt   = if updated > created then updated else created
        }
      }
    
    let projectWithConstraints minNameLen maxNameLen minDescLen maxDescLen =
      gen {
        let! name = 
          Gen.sized (fun _ ->
            nonEmptyString
            |> Gen.filter (fun s -> 
              s.Length >= minNameLen && s.Length <= maxNameLen)
          )
        let! description = 
          Gen.sized (fun _ ->
            nonEmptyString
            |> Gen.filter (fun s -> 
              s.Length >= minDescLen && s.Length <= maxDescLen)
          )
        let! category = projectCategory
        let! status = projectStatus
        let! created = Arb.generate<DateTime>
        let! updated = Arb.generate<DateTime>
        
        return {
          Id          = Guid.NewGuid()
          Name        = name
          Description = description
          Category    = category
          Status      = status
          CreatedAt   = created
          UpdatedAt   = if updated > created then updated else created
        }
      }
    
    type Generators =
      static member Project() = Arb.fromGen validProject
      static member ProjectStatus() = Arb.fromGen projectStatus
      static member ProjectCategory() = Arb.fromGen projectCategory

/// Sample test data constants
module SampleData =
  
  let emptyProject = {
    Id          = Guid.Empty
    Name        = ""
    Description = ""
    Category    = WebApp
    Status      = Idea
    CreatedAt   = DateTime.MinValue
    UpdatedAt   = DateTime.MinValue
  }
  
  let testProject1 = {
    Id          = Guid.Parse("123e4567-e89b-12d3-a456-426614174000")
    Name        = "Test Project 1"
    Description = "A test project for unit testing"
    Category    = WebApp
    Status      = InProgress
    CreatedAt   = DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    UpdatedAt   = DateTime(2024, 1, 15, 0, 0, 0, DateTimeKind.Utc)
  }
  
  let testProject2 = {
    Id          = Guid.Parse("223e4567-e89b-12d3-a456-426614174000")
    Name        = "Test Project 2"
    Description = "Another test project"
    Category    = MobileApp
    Status      = Idea
    CreatedAt   = DateTime(2024, 1, 5, 0, 0, 0, DateTimeKind.Utc)
    UpdatedAt   = DateTime(2024, 1, 10, 0, 0, 0, DateTimeKind.Utc)
  }
  
  let testProject3 = {
    Id          = Guid.Parse("323e4567-e89b-12d3-a456-426614174000")
    Name        = "Test Project 3"
    Description = "Game development project"
    Category    = Game
    Status      = Abandoned
    CreatedAt   = DateTime(2024, 2, 1, 0, 0, 0, DateTimeKind.Utc)
    UpdatedAt   = DateTime(2024, 3, 1, 0, 0, 0, DateTimeKind.Utc)
  }
  
  let testProjects = [ testProject1; testProject2; testProject3 ]
  
  let projectsByStatus =
    Map.ofList [
      Idea, testProject2
      InProgress, testProject1
      Abandoned, testProject3
    ]

/// Common test helpers and assertions
module TestHelpers =
  
  // Helper to create a project with specific values
  let createProject name description category status =
    {
      Id          = Guid.NewGuid()
      Name        = name
      Description = description
      Category    = category
      Status      = status
      CreatedAt   = DateTime.UtcNow
      UpdatedAt   = DateTime.UtcNow
    }
  
  // Validation helpers
  let validProjectName = "Valid Project Name"
  let validProjectDescription = "This is a valid project description."
  
  let invalidProjectName = ""
  let invalidProjectDescription = ""
  
  let tooLongProjectName = String.replicate 101 "x"
  let tooLongProjectDescription = String.replicate 1001 "x"
  
  // Edge case values
  let unicodeProjectName = "プロジェクト 🚀 Test"
  let specialCharsProjectName = "Project!@#$%^&*()"
  let sqlInjectionAttempt = "'; DROP TABLE Projects; --"