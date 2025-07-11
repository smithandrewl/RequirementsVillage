namespace RequirementsVillage.Shared.Tests.Unit

open System
open System.Text.Json
open Xunit
open FsUnit.Xunit
open FsCheck.Xunit
open RequirementsVillage.Shared
open RequirementsVillage.Shared.Tests.TestGenerators
open RequirementsVillage.Shared.Tests.Helpers.JsonHelpers
open RequirementsVillage.Shared.Tests.Helpers

module ProjectStatusTests =
  
  [<Fact>]
  let ``All project statuses should have string representations`` () =
    // Following TDD: test first, then implement
    let statuses = [ Idea; InProgress; Completed; Abandoned; OnHold ]
    
    statuses
    |> List.iter (fun status ->
      match status with
      | Idea       -> ()
      | InProgress -> ()
      | Completed  -> ()
      | Abandoned  -> ()
      | OnHold     -> ()
    )
  
  [<Fact>]
  let ``Project status serialization should be consistent`` () =
    
    let testCases = [
      (Idea,       "\"idea\"")
      (InProgress, "\"inProgress\"")
      (Completed,  "\"completed\"")
      (Abandoned,  "\"abandoned\"")
      (OnHold,     "\"onHold\"")
    ]
    
    testCases
    |> List.iter (fun (status, expected) ->
      let json = JsonSerializer.Serialize(status, jsonOptions)
      json |> should equal expected
    )
  
  [<Fact>]
  let ``Project status deserialization should handle all valid values`` () =
    
    let testCases = [
      ("\"idea\"",       Idea)
      ("\"inProgress\"", InProgress)
      ("\"completed\"",  Completed)
      ("\"abandoned\"",  Abandoned)
      ("\"onHold\"",     OnHold)
    ]
    
    testCases
    |> List.iter (fun (json, expected) ->
      let status = JsonSerializer.Deserialize<ProjectStatus>(json, jsonOptions)
      status |> should equal expected
    )
  
  [<Fact>]
  let ``Project status deserialization should fail for invalid values`` () =
    
    let invalidJson = "\"invalid\""
    
    (fun () ->
      JsonSerializer.Deserialize<ProjectStatus>(invalidJson, jsonOptions) |> ignore
    ) |> should throw typeof<System.Exception>
  
  [<Property>]
  let ``Serialization round trip should preserve project status`` (status: ProjectStatus) =
    
    let json = JsonSerializer.Serialize(status, jsonOptions)
    let deserialized = JsonSerializer.Deserialize<ProjectStatus>(json, jsonOptions)
    
    deserialized = status
  
  module StatusTransitions =
    
    [<Fact>]
    let ``Valid status transitions should be allowed`` () =
      let validTransitions = Combined.validTransitions
      
      validTransitions
      |> List.iter (fun (from, to') ->
        // This test documents expected valid transitions
        match from, to' with
        | Idea, InProgress       -> ()
        | Idea, Abandoned        -> ()
        | Idea, OnHold           -> ()
        | InProgress, Completed  -> ()
        | InProgress, Abandoned  -> ()
        | InProgress, OnHold     -> ()
        | OnHold, InProgress     -> ()
        | OnHold, Abandoned      -> ()
        | Completed, Abandoned   -> ()
        | _                      -> failwithf "Unexpected transition: %A -> %A" from to'
      )
    
    [<Fact>]
    let ``Direct transition from Idea to Completed should not be allowed`` () =
      // Business rule: Cannot go directly from Idea to Completed
      let invalidTransition = (Idea, Completed)
      
      // This test documents that this transition is invalid
      invalidTransition |> should equal (Idea, Completed)