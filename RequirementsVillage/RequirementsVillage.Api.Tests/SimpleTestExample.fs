module RequirementsVillage.Api.Tests.SimpleTestExample

open Xunit
open FsUnit.Xunit
open RequirementsVillage.Api.Models

// Simple example test to verify the test project is set up correctly
[<Fact>]
let ``Project status Idea should have correct value`` () =
  let status = Idea
  status |> should equal Idea

[<Fact>]
let ``Project can be created with valid data`` () =
  let project = {
    Id          = System.Guid.NewGuid()
    Name        = "Test Project"
    Description = "A test project"
    Category    = WebApp
    Status      = Idea
    CreatedAt   = System.DateTime.UtcNow
    UpdatedAt   = System.DateTime.UtcNow
  }
  
  project.Name |> should equal "Test Project"
  project.Status |> should equal Idea