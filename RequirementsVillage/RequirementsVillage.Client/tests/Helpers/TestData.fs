module RequirementsVillage.Client.Tests.Helpers.TestData

open System
open RequirementsVillage.Shared
open Bogus

// Configure Bogus faker
let private faker = Faker()

// Test data generators using Bogus
module Generate =
  
  let projectStatus () =
    faker.PickRandom<ProjectStatus>(
      [| Idea; InProgress; Completed; Abandoned; OnHold |]
    )
  
  let projectCategory () =
    faker.PickRandom<ProjectCategory>(
      [| 
        WebApp
        MobileApp
        Library
        Tool
        Game
        Other(faker.Lorem.Word())
      |]
    )
  
  let project () : Project =
    let createdAt = faker.Date.Past(2)
    {
      Id          = Guid.NewGuid()
      Name        = faker.Lorem.Sentence(3).TrimEnd('.')
      Description = faker.Lorem.Paragraph()
      Category    = projectCategory()
      Status      = projectStatus()
      CreatedAt   = createdAt
      UpdatedAt   = faker.Date.Between(createdAt, DateTime.Now)
    }
  
  let projectWithStatus status =
    { project() with Status = status }
  
  let projectWithCategory category =
    { project() with Category = category }
  
  let projects count =
    [ for _ in 1..count -> project() ]
  
  let projectsWithMixedStatuses () =
    [
      projectWithStatus Idea
      projectWithStatus InProgress
      projectWithStatus Completed
      projectWithStatus Abandoned
      projectWithStatus OnHold
    ]

// Sample test data constants
module Sample =
  
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
    CreatedAt   = DateTime(2024, 1, 1)
    UpdatedAt   = DateTime(2024, 1, 15)
  }
  
  let testProject2 = {
    Id          = Guid.Parse("223e4567-e89b-12d3-a456-426614174000")
    Name        = "Test Project 2"
    Description = "Another test project"
    Category    = MobileApp
    Status      = Idea
    CreatedAt   = DateTime(2024, 1, 5)
    UpdatedAt   = DateTime(2024, 1, 10)
  }
  
  let testProjects = [ testProject1; testProject2 ]