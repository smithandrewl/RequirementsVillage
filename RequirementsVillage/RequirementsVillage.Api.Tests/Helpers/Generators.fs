namespace RequirementsVillage.Api.Tests.Helpers

open System
open Bogus
open FsCheck
open RequirementsVillage.Api.Models

module Generators =
  
  // Initialize Bogus with deterministic seed for reproducible tests
  let private faker = Faker("en")
  
  // Bogus-based generators for realistic test data
  module Bogus =
    
    let projectName() =
      faker.Random.ArrayElement([|
        faker.Commerce.ProductName()
        faker.Company.CatchPhrase()
        sprintf "%s %s" (faker.Hacker.Noun()) (faker.Hacker.Noun())
        sprintf "%s Manager" (faker.Lorem.Word())
        sprintf "%s Tracker" (faker.Lorem.Word())
      |])
    
    let projectDescription() =
      faker.Lorem.Paragraph()
    
    let projectCategory() =
      faker.Random.ArrayElement([|
        WebApp
        MobileApp
        Library
        Tool
        Game
        Other (faker.Lorem.Word())
      |])
    
    let projectStatus() =
      faker.Random.ArrayElement([|
        Idea
        InProgress
        Completed
        Abandoned
        OnHold
      |])
    
    let project() = {
      Id          = System.Guid.NewGuid()
      Name        = projectName()
      Description = projectDescription()
      Category    = projectCategory()
      Status      = projectStatus()
      CreatedAt   = faker.Date.Past(2, DateTime.UtcNow)
      UpdatedAt   = faker.Date.Recent(30)
    }
    
    let projects (count: int) =
      [ for _ in 1..count -> project() ]
    
    let projectWithStatus (status: ProjectStatus) =
      { project() with Status = status }
    
    let projectWithCategory (category: ProjectCategory) =
      { project() with Category = category }
    
    let recentProject() =
      let created = faker.Date.Recent(7)
      { project() with
          CreatedAt = created
          UpdatedAt = faker.Date.Between(created, DateTime.UtcNow) }
    
    let oldProject() =
      let created = faker.Date.Past(2, DateTime.UtcNow)
      { project() with
          CreatedAt = created
          UpdatedAt = faker.Date.Between(created, DateTime.UtcNow.AddMonths(-6)) }
  
  // FsCheck generators for property-based testing
  module FsCheck =
    
    let projectStatusGen =
      Gen.elements [ Idea; InProgress; Completed; Abandoned; OnHold ]
    
    let projectCategoryGen =
      Gen.frequency [
        (5, Gen.constant WebApp)
        (3, Gen.constant MobileApp)
        (2, Gen.constant Library)
        (2, Gen.constant Tool)
        (1, Gen.constant Game)
        (1, Arb.generate<string> |> Gen.filter (fun s -> not (String.IsNullOrWhiteSpace s)) |> Gen.map Other)
      ]
    
    let validProjectNameGen =
      Arb.generate<string>
      |> Gen.filter (fun s -> not (String.IsNullOrWhiteSpace s) && s.Length <= 100)
    
    let validProjectDescriptionGen =
      Arb.generate<string>
      |> Gen.filter (fun s -> not (String.IsNullOrWhiteSpace s) && s.Length <= 1000)
    
    let dateTimeGen =
      gen {
        let! year = Gen.choose (2020, 2025)
        let! month = Gen.choose (1, 12)
        let! day = Gen.choose (1, 28) // Safe for all months
        let! hour = Gen.choose (0, 23)
        let! minute = Gen.choose (0, 59)
        let! second = Gen.choose (0, 59)
        return DateTime(year, month, day, hour, minute, second, DateTimeKind.Utc)
      }
    
    let projectGen =
      gen {
        let! id = Arb.generate<Guid>
        let! name = validProjectNameGen
        let! description = validProjectDescriptionGen
        let! category = projectCategoryGen
        let! status = projectStatusGen
        let! createdAt = dateTimeGen
        let! updatedAt = dateTimeGen |> Gen.filter (fun d -> d >= createdAt)
        
        return {
          Id          = id
          Name        = name
          Description = description
          Category    = category
          Status      = status
          CreatedAt   = createdAt
          UpdatedAt   = updatedAt
        }
      }
    
    let projectListGen =
      Gen.listOf projectGen
    
    // Arbitrary instances for property-based testing
    type ProjectGenerators =
      static member Project() = Arb.fromGen projectGen
      static member ProjectStatus() = Arb.fromGen projectStatusGen
      static member ProjectCategory() = Arb.fromGen projectCategoryGen
    
    // Register custom generators
    let registerGenerators() =
      Arb.register<ProjectGenerators>()
  
  // Combined generators using both Bogus and FsCheck
  module Combined =
    
    let validTransitions = [
      (Idea, InProgress)
      (Idea, Abandoned)
      (Idea, OnHold)
      (InProgress, Completed)
      (InProgress, Abandoned)
      (InProgress, OnHold)
      (OnHold, InProgress)
      (OnHold, Abandoned)
      (Completed, Abandoned)
    ]
    
    let invalidTransitions = [
      (Idea, Completed)  // Cannot go directly from Idea to Completed
    ]
    
    let allStatuses = 
      [ Idea; InProgress; Completed; Abandoned; OnHold ]
    
    let allCategories =
      [ WebApp; MobileApp; Library; Tool; Game; Other "Custom" ]