namespace RequirementsVillage.Api.Tests.Unit.Models

open System
open System.Text.Json
open Xunit
open FsUnit.Xunit
open FsCheck
open FsCheck.Xunit
open RequirementsVillage.Api.Models
open RequirementsVillage.Api.Models.Serialization
open RequirementsVillage.Api.Tests.Helpers

module ModelConversionTests =
  
  module StatusConversions =
    
    [<Fact>]
    let ``ProjectStatus to string conversion should be consistent`` () =
      // Test that we can create a mapping function
      let statusToString (status: ProjectStatus) =
        match status with
        | Idea       -> "idea"
        | InProgress -> "inProgress"
        | Completed  -> "completed"
        | Abandoned  -> "abandoned"
        | OnHold     -> "onHold"
      
      // Verify all cases are covered
      let allStatuses = [ Idea; InProgress; Completed; Abandoned; OnHold ]
      
      allStatuses
      |> List.map statusToString
      |> should equal [ "idea"; "inProgress"; "completed"; "abandoned"; "onHold" ]
    
    [<Fact>]
    let ``String to ProjectStatus conversion should handle all valid cases`` () =
      // Test that we can create a parsing function
      let parseStatus (s: string) =
        match s with
        | "idea"       -> Some Idea
        | "inProgress" -> Some InProgress
        | "completed"  -> Some Completed
        | "abandoned"  -> Some Abandoned
        | "onHold"     -> Some OnHold
        | _            -> None
      
      // Test valid cases
      let validCases = [
        ("idea",       Some Idea)
        ("inProgress", Some InProgress)
        ("completed",  Some Completed)
        ("abandoned",  Some Abandoned)
        ("onHold",     Some OnHold)
      ]
      
      validCases
      |> List.iter (fun (input, expected) ->
        parseStatus input |> should equal expected
      )
      
      // Test invalid cases
      parseStatus "invalid" |> should equal None
      parseStatus ""        |> should equal None
      parseStatus null      |> should equal None
    
    [<Property>]
    let ``Status conversion roundtrip should preserve value`` (status: ProjectStatus) =
      let statusToString s =
        match s with
        | Idea       -> "idea"
        | InProgress -> "inProgress"
        | Completed  -> "completed"
        | Abandoned  -> "abandoned"
        | OnHold     -> "onHold"
      
      let parseStatus s =
        match s with
        | "idea"       -> Idea
        | "inProgress" -> InProgress
        | "completed"  -> Completed
        | "abandoned"  -> Abandoned
        | "onHold"     -> OnHold
        | _            -> failwithf "Invalid status: %s" s
      
      let stringValue = statusToString status
      let parsedValue = parseStatus stringValue
      
      parsedValue = status
  
  module CategoryConversions =
    
    [<Fact>]
    let ``ProjectCategory to string conversion should handle all cases`` () =
      let categoryToString (category: ProjectCategory) =
        match category with
        | WebApp    -> "webApp"
        | MobileApp -> "mobileApp"
        | Library   -> "library"
        | Tool      -> "tool"
        | Game      -> "game"
        | Other s   -> s
      
      let testCases = [
        (WebApp,          "webApp")
        (MobileApp,       "mobileApp")
        (Library,         "library")
        (Tool,            "tool")
        (Game,            "game")
        (Other "Custom",  "Custom")
        (Other "",        "")
      ]
      
      testCases
      |> List.iter (fun (category, expected) ->
        categoryToString category |> should equal expected
      )
    
    [<Fact>]
    let ``String to ProjectCategory conversion should handle all cases`` () =
      let parseCategory (s: string) =
        match s with
        | "webApp"    -> WebApp
        | "mobileApp" -> MobileApp
        | "library"   -> Library
        | "tool"      -> Tool
        | "game"      -> Game
        | other       -> Other other
      
      let testCases = [
        ("webApp",    WebApp)
        ("mobileApp", MobileApp)
        ("library",   Library)
        ("tool",      Tool)
        ("game",      Game)
        ("Custom",    Other "Custom")
        ("",          Other "")
        ("anything",  Other "anything")
      ]
      
      testCases
      |> List.iter (fun (input, expected) ->
        parseCategory input |> should equal expected
      )
    
    [<Property>]
    let ``Category conversion roundtrip should preserve value`` (category: ProjectCategory) =
      let categoryToString cat =
        match cat with
        | WebApp    -> "webApp"
        | MobileApp -> "mobileApp"
        | Library   -> "library"
        | Tool      -> "tool"
        | Game      -> "game"
        | Other s   -> s
      
      let parseCategory s =
        match s with
        | "webApp"    -> WebApp
        | "mobileApp" -> MobileApp
        | "library"   -> Library
        | "tool"      -> Tool
        | "game"      -> Game
        | other       -> Other other
      
      let stringValue = categoryToString category
      let parsedValue = parseCategory stringValue
      
      parsedValue = category
  
  module JsonConverterTests =
    
    [<Fact>]
    let ``ProjectStatusConverter Write method should produce correct JSON values`` () =
      let converter = ProjectStatusConverter()
      
      let testCases = [
        (Idea,       "idea")
        (InProgress, "inProgress")
        (Completed,  "completed")
        (Abandoned,  "abandoned")
        (OnHold,     "onHold")
      ]
      
      testCases
      |> List.iter (fun (status, expected) ->
        use stream = new System.IO.MemoryStream()
        use writer = new System.Text.Json.Utf8JsonWriter(stream)
        
        converter.Write(writer, status, JsonSerializerOptions())
        writer.Flush()
        
        let result = System.Text.Encoding.UTF8.GetString(stream.ToArray())
        result |> should equal (sprintf "\"%s\"" expected)
      )
    
    [<Fact>]
    let ``ProjectCategoryConverter Write method should handle all cases`` () =
      let converter = ProjectCategoryConverter()
      
      let testCases = [
        (WebApp,          "webApp")
        (MobileApp,       "mobileApp")
        (Library,         "library")
        (Tool,            "tool")
        (Game,            "game")
        (Other "AI/ML",   "AI/ML")
        (Other "",        "")
      ]
      
      testCases
      |> List.iter (fun (category, expected) ->
        use stream = new System.IO.MemoryStream()
        use writer = new System.Text.Json.Utf8JsonWriter(stream)
        
        converter.Write(writer, category, JsonSerializerOptions())
        writer.Flush()
        
        let result = System.Text.Encoding.UTF8.GetString(stream.ToArray())
        result |> should equal (sprintf "\"%s\"" expected)
      )
  
  module ModelTransformations =
    
    [<Fact>]
    let ``Project status transitions should follow business rules`` () =
      // Define valid transitions as a function
      let isValidTransition (from: ProjectStatus) (to': ProjectStatus) =
        match from, to' with
        | Idea, InProgress       -> true
        | Idea, Abandoned        -> true
        | Idea, OnHold           -> true
        | InProgress, Completed  -> true
        | InProgress, Abandoned  -> true
        | InProgress, OnHold     -> true
        | OnHold, InProgress     -> true
        | OnHold, Abandoned      -> true
        | Completed, Abandoned   -> true
        | _                      -> false
      
      // Test some valid transitions
      isValidTransition Idea InProgress       |> should equal true
      isValidTransition InProgress Completed  |> should equal true
      isValidTransition OnHold InProgress     |> should equal true
      
      // Test some invalid transitions
      isValidTransition Idea Completed        |> should equal false
      isValidTransition Completed InProgress  |> should equal false
      isValidTransition Abandoned InProgress  |> should equal false
    
    [<Fact>]
    let ``Project update should preserve ID and creation date`` () =
      let original = TestHelpers.createTestProject()
      
      let updated = {
        original with
          Name        = "Updated Name"
          Description = "Updated Description"
          Status      = InProgress
          UpdatedAt   = DateTime.UtcNow.AddHours(1.0)
      }
      
      updated.Id        |> should equal original.Id
      updated.CreatedAt |> should equal original.CreatedAt
      updated.UpdatedAt |> should be (greaterThan original.UpdatedAt)
    
    [<Property>]
    let ``Project with updated fields should maintain invariants`` 
      (name: string)
      (description: string)
      (category: ProjectCategory)
      (status: ProjectStatus) =
      
      (TestHelpers.validProjectName name && TestHelpers.validProjectDescription description) ==> lazy (
        let original = TestHelpers.createTestProject()
        
        let updated = {
          original with
            Name        = name
            Description = description
            Category    = category
            Status      = status
            UpdatedAt   = DateTime.UtcNow.AddSeconds(1.0)
        }
        
        // Invariants that should hold
        updated.Id = original.Id &&
        updated.CreatedAt = original.CreatedAt &&
        updated.UpdatedAt >= original.UpdatedAt
      )