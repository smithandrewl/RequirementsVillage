namespace RequirementsVillage.Shared.Tests.Unit

open System
open Xunit
open FsUnit.Xunit
open FsCheck
open FsCheck.Xunit
open Thoth.Json.Net
open RequirementsVillage.Shared
open RequirementsVillage.Shared.Codecs
open RequirementsVillage.Shared.Tests.Helpers

module CodecsTests =
  
  module ProjectStatusTests =
    
    [<Fact>]
    let ``ProjectStatus encoder should encode all statuses correctly`` () =
      let testCases = [
        (Idea,       "\"idea\"")
        (InProgress, "\"inProgress\"")
        (Completed,  "\"completed\"")
        (Abandoned,  "\"abandoned\"")
        (OnHold,     "\"onHold\"")
      ]
      
      testCases
      |> List.iter (fun (status, expected) ->
        let encoded = Encode.projectStatus status
        let json = Encode.toString 0 encoded
        json |> should equal expected
      )
    
    [<Fact>]
    let ``ProjectStatus decoder should decode all valid status strings`` () =
      let testCases = [
        ("\"idea\"",       Idea)
        ("\"inProgress\"", InProgress)
        ("\"completed\"",  Completed)
        ("\"abandoned\"",  Abandoned)
        ("\"onHold\"",     OnHold)
      ]
      
      testCases
      |> List.iter (fun (json, expected) ->
        let result = Decode.fromString Decode.projectStatus json
        result |> TestHelpers.shouldBeOk |> should equal expected
      )
    
    [<Fact>]
    let ``ProjectStatus decoder should fail for invalid status strings`` () =
      let invalidCases = [
        "\"invalid\""
        "\"IDEA\""
        "\"in-progress\""
        "\"\""
        "null"
      ]
      
      invalidCases
      |> List.iter (fun json ->
        let result = Decode.fromString Decode.projectStatus json
        result |> TestHelpers.shouldBeError |> ignore
      )
    
    [<Property>]
    let ``ProjectStatus round-trip encoding/decoding preserves value`` (status: ProjectStatus) =
      let encoded = Encode.projectStatus status
      let json = Encode.toString 0 encoded
      let decoded = Decode.fromString Decode.projectStatus json
      
      decoded |> TestHelpers.shouldBeOk |> should equal status
  
  module ProjectCategoryTests =
    
    [<Fact>]
    let ``ProjectCategory encoder should encode all categories correctly`` () =
      let testCases = [
        (WebApp,        "\"webApp\"")
        (MobileApp,     "\"mobileApp\"")
        (Library,       "\"library\"")
        (Tool,          "\"tool\"")
        (Game,          "\"game\"")
        (Other "Custom", "\"Custom\"")
      ]
      
      testCases
      |> List.iter (fun (category, expected) ->
        let encoded = Encode.projectCategory category
        let json = Encode.toString 0 encoded
        json |> should equal expected
      )
    
    [<Fact>]
    let ``ProjectCategory decoder should decode all valid category strings`` () =
      let testCases = [
        ("\"webApp\"",    WebApp)
        ("\"mobileApp\"", MobileApp)
        ("\"library\"",   Library)
        ("\"tool\"",      Tool)
        ("\"game\"",      Game)
        ("\"Custom\"",    Other "Custom")
        ("\"anything\"",  Other "anything")
      ]
      
      testCases
      |> List.iter (fun (json, expected) ->
        let result = Decode.fromString Decode.projectCategory json
        result |> TestHelpers.shouldBeOk |> should equal expected
      )
    
    [<Fact>]
    let ``ProjectCategory decoder should handle empty string as Other`` () =
      let result = Decode.fromString Decode.projectCategory "\"\""
      result |> TestHelpers.shouldBeOk |> should equal (Other "")
    
    [<Property>]
    let ``ProjectCategory round-trip encoding/decoding preserves value`` (category: ProjectCategory) =
      // Filter out null categories since they're not valid
      match category with
      | Other null -> true // Skip null categories
      | _ ->
        let encoded = Encode.projectCategory category
        let json = Encode.toString 0 encoded
        let decoded = Decode.fromString Decode.projectCategory json
        
        decoded |> TestHelpers.shouldBeOk |> should equal category
        true
  
  module ProjectTests =
    
    [<Fact>]
    let ``Project encoder should encode a complete project correctly`` () =
      let project = {
        Id          = Guid.Parse("12345678-1234-1234-1234-123456789012")
        Name        = "Test Project"
        Description = "A test project description"
        Category    = WebApp
        Status      = InProgress
        CreatedAt   = DateTime(2023, 1, 1, 12, 0, 0, DateTimeKind.Utc)
        UpdatedAt   = DateTime(2023, 1, 2, 12, 0, 0, DateTimeKind.Utc)
      }
      
      let encoded = Encode.project project
      let json = Encode.toString 2 encoded
      
      // Verify JSON structure
      json |> should haveSubstring "\"id\": \"12345678-1234-1234-1234-123456789012\""
      json |> should haveSubstring "\"name\": \"Test Project\""
      json |> should haveSubstring "\"description\": \"A test project description\""
      json |> should haveSubstring "\"category\": \"webApp\""
      json |> should haveSubstring "\"status\": \"inProgress\""
      json |> should haveSubstring "\"createdAt\":"
      json |> should haveSubstring "\"updatedAt\":"
    
    [<Fact>]
    let ``Project decoder should decode a complete project JSON`` () =
      let json = """
        {
          "id": "12345678-1234-1234-1234-123456789012",
          "name": "Test Project",
          "description": "A test project description",
          "category": "webApp",
          "status": "inProgress",
          "createdAt": "2023-01-01T12:00:00Z",
          "updatedAt": "2023-01-02T12:00:00Z"
        }
      """
      
      let result = Decode.fromString Decode.project json
      let project = result |> TestHelpers.shouldBeOk
      
      project.Id          |> should equal (Guid.Parse("12345678-1234-1234-1234-123456789012"))
      project.Name        |> should equal "Test Project"
      project.Description |> should equal "A test project description"
      project.Category    |> should equal WebApp
      project.Status      |> should equal InProgress
      project.CreatedAt.Kind |> should equal DateTimeKind.Utc
      project.UpdatedAt.Kind |> should equal DateTimeKind.Utc
    
    [<Fact>]
    let ``Project decoder should fail for missing required fields`` () =
      let invalidCases = [
        // Missing id
        """{ "name": "Test", "description": "Desc", "category": "webApp", "status": "idea", "createdAt": "2023-01-01T12:00:00Z", "updatedAt": "2023-01-01T12:00:00Z" }"""
        // Missing name
        """{ "id": "12345678-1234-1234-1234-123456789012", "description": "Desc", "category": "webApp", "status": "idea", "createdAt": "2023-01-01T12:00:00Z", "updatedAt": "2023-01-01T12:00:00Z" }"""
        // Invalid status
        """{ "id": "12345678-1234-1234-1234-123456789012", "name": "Test", "description": "Desc", "category": "webApp", "status": "invalid", "createdAt": "2023-01-01T12:00:00Z", "updatedAt": "2023-01-01T12:00:00Z" }"""
      ]
      
      invalidCases
      |> List.iter (fun json ->
        let result = Decode.fromString Decode.project json
        result |> TestHelpers.shouldBeError |> ignore
      )
    
    [<Property>]
    let ``Project round-trip encoding/decoding preserves all fields`` () =
      let project = Generators.Bogus.project() // Use Bogus generator for more predictable test data
      
      let encoded = Encode.project project
      let json = Encode.toString 0 encoded
      let decoded = Decode.fromString Decode.project json
      
      let decodedProject = decoded |> TestHelpers.shouldBeOk
      
      decodedProject.Id          |> should equal project.Id
      decodedProject.Name        |> should equal project.Name
      decodedProject.Description |> should equal project.Description
      decodedProject.Category    |> should equal project.Category
      decodedProject.Status      |> should equal project.Status
      // DateTime comparison - The dates should be very close but might have different timezone info
      // Just check that they represent the same point in time
      decodedProject.CreatedAt.ToUniversalTime().ToString("O") |> should equal (project.CreatedAt.ToUniversalTime().ToString("O"))
      decodedProject.UpdatedAt.ToUniversalTime().ToString("O") |> should equal (project.UpdatedAt.ToUniversalTime().ToString("O"))
  
  module ProjectErrorTests =
    
    [<Fact>]
    let ``ProjectError encoder should encode NotFound error correctly`` () =
      let projectId = Guid.Parse("12345678-1234-1234-1234-123456789012")
      let error = NotFound(projectId, "dashboard search")
      
      let encoded = Encode.projectError error
      let json = Encode.toString 2 encoded
      
      json |> should haveSubstring "\"type\": \"NotFound\""
      json |> should haveSubstring "\"projectId\": \"12345678-1234-1234-1234-123456789012\""
      json |> should haveSubstring "\"searchContext\": \"dashboard search\""
    
    [<Fact>]
    let ``ProjectError encoder should encode ValidationFailed error correctly`` () =
      let error = ValidationFailed("name", "too long", "A very long project name that exceeds the limit")
      
      let encoded = Encode.projectError error
      let json = Encode.toString 2 encoded
      
      json |> should haveSubstring "\"type\": \"ValidationFailed\""
      json |> should haveSubstring "\"field\": \"name\""
      json |> should haveSubstring "\"reason\": \"too long\""
      json |> should haveSubstring "\"attemptedValue\":"
    
    [<Fact>]
    let ``ProjectError encoder should encode DatabaseError correctly`` () =
      let error = DatabaseError("INSERT", "Projects", exn "Connection timeout")
      
      let encoded = Encode.projectError error
      let json = Encode.toString 2 encoded
      
      json |> should haveSubstring "\"type\": \"DatabaseError\""
      json |> should haveSubstring "\"operation\": \"INSERT\""
      json |> should haveSubstring "\"tableName\": \"Projects\""
      json |> should haveSubstring "\"error\": \"Connection timeout\""
    
    [<Fact>]
    let ``ProjectError encoder should encode UnknownError correctly`` () =
      let error = UnknownError "Something unexpected happened"
      
      let encoded = Encode.projectError error
      let json = Encode.toString 2 encoded
      
      json |> should haveSubstring "\"type\": \"UnknownError\""
      json |> should haveSubstring "\"message\": \"Something unexpected happened\""
    
    [<Fact>]
    let ``ProjectError decoder should decode NotFound error correctly`` () =
      let json = """
        {
          "type": "NotFound",
          "projectId": "12345678-1234-1234-1234-123456789012",
          "searchContext": "dashboard search"
        }
      """
      
      let result = Decode.fromString Decode.projectError json
      let error = result |> TestHelpers.shouldBeOk
      
      match error with
      | NotFound (id, context) ->
        id      |> should equal (Guid.Parse("12345678-1234-1234-1234-123456789012"))
        context |> should equal "dashboard search"
      | _ -> failwith "Expected NotFound error"
    
    [<Fact>]
    let ``ProjectError decoder should decode ValidationFailed error correctly`` () =
      let json = """
        {
          "type": "ValidationFailed",
          "field": "name",
          "reason": "too long",
          "attemptedValue": "A very long project name"
        }
      """
      
      let result = Decode.fromString Decode.projectError json
      let error = result |> TestHelpers.shouldBeOk
      
      match error with
      | ValidationFailed (field, reason, value) ->
        field  |> should equal "name"
        reason |> should equal "too long"
        // Value is decoded as JsonValue, so we check it's not null
        value  |> should not' (be null)
      | _ -> failwith "Expected ValidationFailed error"
    
    [<Fact>]
    let ``ProjectError decoder should decode DatabaseError correctly`` () =
      let json = """
        {
          "type": "DatabaseError",
          "operation": "INSERT",
          "tableName": "Projects",
          "error": "Connection timeout"
        }
      """
      
      let result = Decode.fromString Decode.projectError json
      let error = result |> TestHelpers.shouldBeOk
      
      match error with
      | DatabaseError (op, table, ex) ->
        op    |> should equal "INSERT"
        table |> should equal "Projects"
        ex.Message |> should equal "Connection timeout"
      | _ -> failwith "Expected DatabaseError"
    
    [<Fact>]
    let ``ProjectError decoder should decode UnknownError correctly`` () =
      let json = """
        {
          "type": "UnknownError",
          "message": "Something unexpected happened"
        }
      """
      
      let result = Decode.fromString Decode.projectError json
      let error = result |> TestHelpers.shouldBeOk
      
      match error with
      | UnknownError msg ->
        msg |> should equal "Something unexpected happened"
      | _ -> failwith "Expected UnknownError"
    
    [<Fact>]
    let ``ProjectError decoder should fail for unknown error types`` () =
      let json = """
        {
          "type": "InvalidErrorType",
          "data": "some data"
        }
      """
      
      let result = Decode.fromString Decode.projectError json
      result |> TestHelpers.shouldBeError |> ignore
    
    [<Fact>]
    let ``ProjectError round-trip encoding/decoding preserves all error types`` () =
      let errors = [
        NotFound(Guid.NewGuid(), "test context")
        ValidationFailed("field", "reason", "value")
        DatabaseError("SELECT", "Projects", exn "DB error")
        UnknownError "Unknown error message"
      ]
      
      errors
      |> List.iter (fun error ->
        let encoded = Encode.projectError error
        let json = Encode.toString 0 encoded
        let decoded = Decode.fromString Decode.projectError json
        let decodedError = decoded |> TestHelpers.shouldBeOk
        
        match error, decodedError with
        | NotFound (id1, ctx1), NotFound (id2, ctx2) ->
          id1 |> should equal id2
          ctx1 |> should equal ctx2
        | ValidationFailed (f1, r1, _), ValidationFailed (f2, r2, _) ->
          f1 |> should equal f2
          r1 |> should equal r2
        | DatabaseError (o1, t1, e1), DatabaseError (o2, t2, e2) ->
          o1 |> should equal o2
          t1 |> should equal t2
          e1.Message |> should equal e2.Message
        | UnknownError m1, UnknownError m2 ->
          m1 |> should equal m2
        | _ -> failwith "Error type mismatch in round-trip test"
      )
  
  module EdgeCaseTests =
    
    [<Fact>]
    let ``Decoders should handle malformed JSON gracefully`` () =
      let malformedCases = [
        ""                     // Empty string
        "{"                    // Incomplete JSON
        "null"                 // Null value  
        "[1,2,3]"              // Array instead of object
        "{invalid json"        // Invalid syntax
      ]
      
      malformedCases
      |> List.iter (fun json ->
        // projectStatus and projectError should return Error for malformed JSON
        Decode.fromString Decode.projectStatus json    |> TestHelpers.shouldBeError |> ignore
        Decode.fromString Decode.project json          |> TestHelpers.shouldBeError |> ignore
        Decode.fromString Decode.projectError json     |> TestHelpers.shouldBeError |> ignore
        
        // projectCategory accepts any string, so "just a string" would be valid
        if json <> "\"just a string\"" then
          Decode.fromString Decode.projectCategory json  |> TestHelpers.shouldBeError |> ignore
      )
    
    [<Fact>]
    let ``Project with Other category containing special characters`` () =
      let specialCategories = [
        Other "Category/With/Slashes"
        Other "Category\"With\"Quotes"
        Other "Category\\With\\Backslashes"
        Other "Category\nWith\nNewlines"
        Other "Category\tWith\tTabs"
        Other "类别" // Unicode characters
      ]
      
      specialCategories
      |> List.iter (fun category ->
        let project = { TestHelpers.createTestProject() with Category = category }
        let encoded = Encode.project project
        let json = Encode.toString 0 encoded
        let decoded = Decode.fromString Decode.project json
        
        let decodedProject = decoded |> TestHelpers.shouldBeOk
        decodedProject.Category |> should equal category
      )
    
    [<Fact>]
    let ``Extremely long strings should be handled correctly`` () =
      let longString = String.replicate 10000 "x"
      let project = {
        TestHelpers.createTestProject() with
          Name = longString
          Description = longString
          Category = Other longString
      }
      
      let encoded = Encode.project project
      let json = Encode.toString 0 encoded
      let decoded = Decode.fromString Decode.project json
      
      let decodedProject = decoded |> TestHelpers.shouldBeOk
      decodedProject.Name        |> should equal longString
      decodedProject.Description |> should equal longString
      match decodedProject.Category with
      | Other s -> s |> should equal longString
      | _ -> failwith "Expected Other category"
    
    [<Fact>]
    let ``DateTime edge cases should be handled correctly`` () =
      let edgeCases = [
        DateTime.MinValue.ToUniversalTime()
        DateTime.MaxValue.ToUniversalTime()
        DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc)
        DateTime.UtcNow
      ]
      
      edgeCases
      |> List.iter (fun dt ->
        let project = {
          TestHelpers.createTestProject() with
            CreatedAt = dt
            UpdatedAt = dt
        }
        
        let encoded = Encode.project project
        let json = Encode.toString 0 encoded
        let decoded = Decode.fromString Decode.project json
        
        let decodedProject = decoded |> TestHelpers.shouldBeOk
        // Compare DateTime values as UTC strings
        decodedProject.CreatedAt.ToUniversalTime().ToString("O") |> should equal (dt.ToUniversalTime().ToString("O"))
        decodedProject.UpdatedAt.ToUniversalTime().ToString("O") |> should equal (dt.ToUniversalTime().ToString("O"))
      )