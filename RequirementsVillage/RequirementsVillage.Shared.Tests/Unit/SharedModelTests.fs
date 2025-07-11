namespace RequirementsVillage.Shared.Tests.Unit

open System
open System.Text.Json
open Xunit
open FsUnit.Xunit
open RequirementsVillage.Shared
open RequirementsVillage.Shared.Tests.Helpers.JsonHelpers
open RequirementsVillage.Shared.Tests.Helpers
open RequirementsVillage.Shared.Tests.TestGenerators

/// Tests for shared domain models and their conversions
module SharedModelTests =
  
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
  let ``ProjectStatus to string conversion should be consistent`` () =
    let statusToString (status: ProjectStatus) =
      match status with
      | Idea       -> "idea"
      | InProgress -> "inProgress"
      | Completed  -> "completed"
      | Abandoned  -> "abandoned"
      | OnHold     -> "onHold"
    
    let allStatuses = [ Idea; InProgress; Completed; Abandoned; OnHold ]
    
    allStatuses
    |> List.map statusToString
    |> should equal [ "idea"; "inProgress"; "completed"; "abandoned"; "onHold" ]
  
  [<Fact>]
  let ``ProjectCategory to string conversion should handle all cases`` () =
    let categoryToString (category: ProjectCategory) =
      match category with
      | WebApp    -> "webApp"
      | MobileApp -> "mobileApp"
      | Library   -> "library"
      | Tool      -> "tool"
      | Game      -> "game"
      | Other s   -> if System.String.IsNullOrEmpty(s) then "other" else s
    
    let testCases = [
      (WebApp,          "webApp")
      (MobileApp,       "mobileApp")
      (Library,         "library")
      (Tool,            "tool")
      (Game,            "game")
      (Other "Custom",  "Custom")
      (Other "",        "other")
    ]
    
    testCases
    |> List.iter (fun (category, expected) ->
      categoryToString category |> should equal expected
    )
  
  [<Fact>]
  let ``Bogus should generate valid projects`` () =
    let projects = TestDataGenerators.Bogus.Default.projects 5
    
    projects |> should haveLength 5
    
    projects
    |> List.iter (fun project ->
      TestHelpers.isValidProjectName project.Name |> should equal true
      TestHelpers.isValidProjectDescription project.Description |> should equal true
      project.UpdatedAt |> should be (greaterThanOrEqualTo project.CreatedAt)
    )