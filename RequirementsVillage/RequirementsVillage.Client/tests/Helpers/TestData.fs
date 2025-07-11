module RequirementsVillage.Client.Tests.Helpers.TestData

open System
open RequirementsVillage.Shared

// Simple test data generators for client tests
module Generate =
  let private random = Random(12345) // Fixed seed for deterministic tests
  
  let projectStatus () =
    let statuses = [| Idea; InProgress; Completed; Abandoned; OnHold |]
    statuses.[random.Next(statuses.Length)]
  
  let projectCategory () =
    let categories = [| WebApp; MobileApp; Library; Tool; Game; Other "" |]
    categories.[random.Next(categories.Length)]
  
  let project () : Project =
    let id = Guid.NewGuid()
    let now = DateTime.UtcNow
    {
      Id = id
      Name = sprintf "Test Project %s" (id.ToString().Substring(0, 8))
      Description = "A test project description"
      Category = projectCategory()
      Status = projectStatus()
      CreatedAt = now.AddDays(float(-random.Next(1, 365)))
      UpdatedAt = now
    }
  
  let projects count =
    List.init count (fun _ -> project())

// Predefined sample data for tests
module Sample =
  let testProject1 = {
    Id = Guid.Parse("00000000-0000-0000-0000-000000000001")
    Name = "Test Project 1"
    Description = "A test project for unit tests"
    Category = WebApp
    Status = InProgress
    CreatedAt = DateTime(2023, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    UpdatedAt = DateTime(2023, 1, 15, 0, 0, 0, DateTimeKind.Utc)
  }
  
  let testProject2 = {
    Id = Guid.Parse("00000000-0000-0000-0000-000000000002")
    Name = "Test Project 2"
    Description = "Another test project"
    Category = Library
    Status = Completed
    CreatedAt = DateTime(2023, 2, 1, 0, 0, 0, DateTimeKind.Utc)
    UpdatedAt = DateTime(2023, 2, 28, 0, 0, 0, DateTimeKind.Utc)
  }
  
  let testProject3 = {
    Id = Guid.Parse("00000000-0000-0000-0000-000000000003")
    Name = "Test Project 3"
    Description = "Yet another test project"
    Category = Tool
    Status = Idea
    CreatedAt = DateTime(2023, 3, 1, 0, 0, 0, DateTimeKind.Utc)
    UpdatedAt = DateTime(2023, 3, 31, 0, 0, 0, DateTimeKind.Utc)
  }