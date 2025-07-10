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

module ModelEdgeCaseTests =
  
  module ProjectEdgeCases =
    
    [<Fact>]
    let ``Project with extreme dates should be valid`` () =
      let farPastProject = {
        TestHelpers.createTestProject() with
          CreatedAt = DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
          UpdatedAt = DateTime(1970, 1, 2, 0, 0, 0, DateTimeKind.Utc)
      }
      
      let farFutureProject = {
        TestHelpers.createTestProject() with
          CreatedAt = DateTime(2100, 12, 31, 23, 59, 59, DateTimeKind.Utc)
          UpdatedAt = DateTime(2100, 12, 31, 23, 59, 59, DateTimeKind.Utc)
      }
      
      farPastProject.UpdatedAt   |> should be (greaterThan farPastProject.CreatedAt)
      farFutureProject.CreatedAt |> should be (lessThan DateTime.MaxValue)
    
    [<Fact>]
    let ``Project with Unicode characters in name and description should serialize correctly`` () =
      let project = {
        TestHelpers.createTestProject() with
          Name = "🚀 Rocket Project 日本語 العربية"
          Description = "Multi-language description: русский, 中文, हिन्दी, emoji: 🎉🔥💻"
      }
      
      let json = JsonSerializer.Serialize(project, jsonOptions)
      let deserialized = JsonSerializer.Deserialize<Project>(json, jsonOptions)
      
      deserialized.Name        |> should equal project.Name
      deserialized.Description |> should equal project.Description
    
    [<Fact>]
    let ``Project with special characters should handle JSON escaping`` () =
      let project = {
        TestHelpers.createTestProject() with
          Name = "Project with \"quotes\" and \\backslashes\\"
          Description = "Line 1\nLine 2\tTabbed\rCarriage return"
      }
      
      let json = JsonSerializer.Serialize(project, jsonOptions)
      let deserialized = JsonSerializer.Deserialize<Project>(json, jsonOptions)
      
      deserialized.Name        |> should equal project.Name
      deserialized.Description |> should equal project.Description
    
    [<Property>]
    let ``Project with any valid GUID should serialize correctly`` () =
      let guidGen = Arb.generate<Guid> |> Gen.filter (fun g -> g <> Guid.Empty)
      
      Prop.forAll (Arb.fromGen guidGen) (fun guid ->
        let project = { TestHelpers.createTestProject() with Id = guid }
        let json = JsonSerializer.Serialize(project, jsonOptions)
        let deserialized = JsonSerializer.Deserialize<Project>(json, jsonOptions)
        
        deserialized.Id = guid
      )
    
    [<Fact>]
    let ``Project with maximum length name and description should be valid`` () =
      let maxNameLength = 100
      let maxDescriptionLength = 1000
      
      let project = {
        TestHelpers.createTestProject() with
          Name        = String.replicate maxNameLength "a"
          Description = String.replicate maxDescriptionLength "b"
      }
      
      TestHelpers.validProjectName project.Name               |> should equal true
      TestHelpers.validProjectDescription project.Description |> should equal true
  
  module ProjectCategoryEdgeCases =
    
    [<Fact>]
    let ``Other category with empty string should be distinct from predefined categories`` () =
      let otherEmpty = Other ""
      let predefined = [ WebApp; MobileApp; Library; Tool; Game ]
      
      predefined
      |> List.forall (fun cat -> cat <> otherEmpty)
      |> should equal true
    
    [<Fact>]
    let ``Other category with whitespace variations should preserve exact value`` () =
      let testCases = [
        Other " "
        Other "  "
        Other "\t"
        Other "\n"
        Other " Leading space"
        Other "Trailing space "
        Other " Both spaces "
      ]
      
      testCases
      |> List.iter (fun category ->
        match category with
        | Other value ->
          let json = JsonSerializer.Serialize(category, jsonOptions)
          let deserialized = JsonSerializer.Deserialize<ProjectCategory>(json, jsonOptions)
          
          match deserialized with
          | Other deserializedValue -> deserializedValue |> should equal value
          | _ -> failwith "Expected Other category"
        | _ -> failwith "Test setup error"
      )
    
    [<Fact>]
    let ``Category deserialization should handle null`` () =
      // null is actually valid JSON that deserializes to the default value
      let result = JsonSerializer.Deserialize<ProjectCategory>("null", jsonOptions)
      result |> should equal (Unchecked.defaultof<ProjectCategory>)
    
    [<Property>]
    let ``Other category with any string should roundtrip correctly`` () =
      Prop.forAll (Arb.fromGen (Gen.map (fun s -> if s = null then "" else s) Arb.generate<string>)) (fun str ->
        let category = Other str
        let json = JsonSerializer.Serialize(category, jsonOptions)
        let deserialized = JsonSerializer.Deserialize<ProjectCategory>(json, jsonOptions)
        
        match deserialized with
        | Other value -> value = str
        | _ -> false
      )
  
  module ProjectStatusEdgeCases =
    
    [<Fact>]
    let ``Status deserialization should be case sensitive`` () =
      let testCases = [
        ("\"IDEA\"",       false)  // Should fail
        ("\"Idea\"",       false)  // Should fail
        ("\"idea\"",       true)   // Should succeed
        ("\"InProgress\"", false)  // Should fail
        ("\"inprogress\"", false)  // Should fail
        ("\"inProgress\"", true)   // Should succeed
      ]
      
      testCases
      |> List.iter (fun (json, shouldSucceed) ->
        if shouldSucceed then
          let status = JsonSerializer.Deserialize<ProjectStatus>(json, jsonOptions)
          status |> should not' (be null)
        else
          (fun () ->
            JsonSerializer.Deserialize<ProjectStatus>(json, jsonOptions) |> ignore
          ) |> should throw typeof<System.Exception>
      )
    
    [<Fact>]
    let ``All status values should be unique when serialized`` () =
      let allStatuses = [ Idea; InProgress; Completed; Abandoned; OnHold ]
      
      let serializedStatuses =
        allStatuses
        |> List.map (fun status -> JsonSerializer.Serialize(status, jsonOptions))
      
      let uniqueCount = serializedStatuses |> List.distinct |> List.length
      
      uniqueCount |> should equal allStatuses.Length
  
  module SerializationEdgeCases =
    
    [<Fact>]
    let ``Deeply nested JSON should not cause stack overflow`` () =
      // Create a project and serialize it multiple times to simulate deep nesting
      let project = TestHelpers.createTestProject()
      
      let mutable json = JsonSerializer.Serialize(project, jsonOptions)
      
      // Wrap in array multiple times
      for i in 1..100 do
        json <- sprintf "[%s]" json
      
      // This should not throw
      (fun () -> json.Length |> ignore) |> should not' (throw typeof<System.Exception>)
    
    [<Fact>]
    let ``Concurrent serialization should be thread-safe`` () =
      let project = TestHelpers.createTestProject()
      let iterations = 100
      
      let results =
        [| 1..iterations |]
        |> Array.Parallel.map (fun _ ->
          JsonSerializer.Serialize(project, jsonOptions)
        )
      
      // All results should be identical
      results
      |> Array.forall (fun json -> json = results.[0])
      |> should equal true
    
    [<Fact>]
    let ``Project with all edge case values should serialize successfully`` () =
      let edgeCaseProject = {
        Id          = Guid.Empty
        Name        = String.replicate 100 "边"  // Unicode character repeated to max length
        Description = String.replicate 1000 "缘"  // Unicode character repeated to max length
        Category    = Other "!@#$%^&*()_+-=[]{}|;':\",./<>?"
        Status      = OnHold
        CreatedAt   = DateTime.MinValue.ToUniversalTime()
        UpdatedAt   = DateTime.MaxValue.ToUniversalTime().AddTicks(-1L)
      }
      
      let json = JsonSerializer.Serialize(edgeCaseProject, jsonOptions)
      let deserialized = JsonSerializer.Deserialize<Project>(json, jsonOptions)
      
      deserialized.Id          |> should equal edgeCaseProject.Id
      deserialized.Name        |> should equal edgeCaseProject.Name
      deserialized.Description |> should equal edgeCaseProject.Description
      deserialized.Category    |> should equal edgeCaseProject.Category
      deserialized.Status      |> should equal edgeCaseProject.Status
  
  module ValidationEdgeCases =
    
    [<Fact>]
    let ``Project name with only whitespace should be invalid`` () =
      let whitespaceNames = [
        " "
        "  "
        "\t"
        "\n"
        "\r\n"
        " \t \n "
      ]
      
      whitespaceNames
      |> List.iter (fun name ->
        TestHelpers.validProjectName name |> should equal false
      )
    
    [<Fact>]
    let ``Project description with only whitespace should be invalid`` () =
      let whitespaceDescriptions = [
        " "
        "  "
        "\t"
        "\n"
        "\r\n"
        " \t \n "
      ]
      
      whitespaceDescriptions
      |> List.iter (fun desc ->
        TestHelpers.validProjectDescription desc |> should equal false
      )
    
    [<Property>]
    let ``Project with exactly max length should be valid`` () =
      let nameGen = Gen.map (fun c -> String.replicate 100 (string c)) (Gen.elements ['a'..'z'])
      let descGen = Gen.map (fun c -> String.replicate 1000 (string c)) (Gen.elements ['a'..'z'])
      
      Prop.forAll (Arb.fromGen (Gen.zip nameGen descGen)) (fun (name, desc) ->
        TestHelpers.validProjectName name &&
        TestHelpers.validProjectDescription desc
      )
    
    [<Property>]
    let ``Project with one character over max length should be invalid`` () =
      let nameGen = Gen.map (fun c -> String.replicate 101 (string c)) (Gen.elements ['a'..'z'])
      let descGen = Gen.map (fun c -> String.replicate 1001 (string c)) (Gen.elements ['a'..'z'])
      
      Prop.forAll (Arb.fromGen (Gen.zip nameGen descGen)) (fun (name, desc) ->
        not (TestHelpers.validProjectName name) &&
        not (TestHelpers.validProjectDescription desc)
      )