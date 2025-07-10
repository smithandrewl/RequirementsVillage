namespace RequirementsVillage.Shared.Tests.Unit

open System
open System.Text.Json
open Xunit
open FsUnit.Xunit
open FsCheck
open FsCheck.Xunit
open RequirementsVillage.Shared
open RequirementsVillage.Shared.Tests.Helpers.JsonHelpers
open RequirementsVillage.Shared.Tests.Helpers

module ProjectCategoryTests =
  
  [<Fact>]
  let ``All predefined project categories should be distinct`` () =
    let categories = [ WebApp; MobileApp; Library; Tool; Game ]
    
    categories
    |> List.distinct
    |> List.length
    |> should equal categories.Length
  
  [<Fact>]
  let ``Other category should accept custom values`` () =
    let customCategories = [
      Other "AI/ML"
      Other "IoT"
      Other "Blockchain"
      Other "Data Science"
    ]
    
    customCategories
    |> List.iter (fun category ->
      match category with
      | Other value -> value |> should not' (be NullOrEmptyString)
      | _           -> failwith "Expected Other category"
    )
  
  [<Fact>]
  let ``Project category serialization should be consistent`` () =
    
    let testCases = [
      (WebApp,          "\"webApp\"")
      (MobileApp,       "\"mobileApp\"")
      (Library,         "\"library\"")
      (Tool,            "\"tool\"")
      (Game,            "\"game\"")
      (Other "Custom",  "\"Custom\"")
      (Other "AI Tool", "\"AI Tool\"")
    ]
    
    testCases
    |> List.iter (fun (category, expected) ->
      let json = JsonSerializer.Serialize(category, jsonOptions)
      json |> should equal expected
    )
  
  [<Fact>]
  let ``Project category deserialization should handle all valid values`` () =
    
    let testCases = [
      ("\"webApp\"",    WebApp)
      ("\"mobileApp\"", MobileApp)
      ("\"library\"",   Library)
      ("\"tool\"",      Tool)
      ("\"game\"",      Game)
      ("\"Custom\"",    Other "Custom")
      ("\"AI Tool\"",   Other "AI Tool")
    ]
    
    testCases
    |> List.iter (fun (json, expected) ->
      let category = JsonSerializer.Deserialize<ProjectCategory>(json, jsonOptions)
      category |> should equal expected
    )
  
  [<Property>]
  let ``Serialization round trip should preserve project category`` (category: ProjectCategory) =
    // Filter out null Other values since they can't be serialized properly
    let isValid = 
      match category with
      | Other s -> s <> null
      | _ -> true
    
    isValid ==> lazy (
      let json = JsonSerializer.Serialize(category, jsonOptions)
      let deserialized = JsonSerializer.Deserialize<ProjectCategory>(json, jsonOptions)
      
      deserialized = category
    )
  
  [<Property>]
  let ``Other category with non-empty string should be valid`` (s: string) =
    (not (System.String.IsNullOrWhiteSpace(s))) ==> lazy (
      let category = Other s
      match category with
      | Other value -> value = s
      | _           -> false
    )